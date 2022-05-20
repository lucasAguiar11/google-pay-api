// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

using GooglePay.PaymentDataCryptography.Models;

namespace GooglePay.PaymentDataCryptography
{
    /// <summary>
    /// An implementation of the recipient side of Google Pay API Payment data cryptography.
    ///
    /// This implementation currently only supports version ECv1.
    /// </summary>
    public class PaymentMethodTokenRecipient
    {
        private const string ECv1 = "ECv1";
        private const string ECv2 = "ECv2";

        private const int SymmetricKeySizeECv1 = 16;
        private const int MacKeySizeECv1 = 16;
        private const int SymmetricKeySizeECv2 = 32;
        private const int MacKeySizeECv2 = 32;

        private const string GoogleSenderId = "Google";

        private readonly string _recipientId;
        private readonly ISignatureKeyProvider _signatureKeyProvider;
        private readonly SignatureVerification _signatureVerification = new SignatureVerification();

        private List<ECPrivateKeyParameters> _privateKeys = new List<ECPrivateKeyParameters>();

        /// <param name="recipientId">Your recipient_id, as provided by Google</param>
        /// <param name="signatureKeyProvider">Provider of public keys for ECDSA signature verification</param>
        public PaymentMethodTokenRecipient(string recipientId, ISignatureKeyProvider signatureKeyProvider)
        {
            _recipientId = recipientId;
            _signatureKeyProvider = signatureKeyProvider;
        }

        internal PaymentMethodTokenRecipient(string recipientId, ISignatureKeyProvider signatureKeyProvider, Util.IClock clock)
        {
            _recipientId = recipientId;
            _signatureKeyProvider = signatureKeyProvider;
            _signatureVerification = new SignatureVerification(clock);
        }

        /// <summary>
        /// Adds an elliptic-curve private key on the NIST P-256 curve. Multiple private keys can be added to support
        /// graceful key rotations.
        /// </summary>
        /// <param name="privateKey">Elliptic-curve private key</param>
        public void AddPrivateKey(ECPrivateKeyParameters privateKey)
        {
            if (!KeyParser.ValidateCurve(privateKey))
            {
                throw new ArgumentException("Invalid private key format or not on NIST P-256 curve", "privateKey");
            }
            _privateKeys.Add(privateKey);
        }

        /// <summary>
        /// Adds an Elliptic Curve private key on the NIST P-256 curve. Multiple private keys can be added to support
        /// graceful key rotations.
        /// </summary>
        /// <param name="privateKeyBytes">Elliptic-curve private key encoded in the ASN.1 byte format</param>
        public void AddPrivateKey(byte[] privateKeyBytes) =>
            _privateKeys.Add(KeyParser.ParsePrivateKeyDer(privateKeyBytes));

        /// <summary>
        /// Adds an Elliptic Curve private key on the NIST P-256 curve. Multiple private keys can be added to support
        /// graceful key rotations.
        /// </summary>
        /// <param name="privateKeyBase64">Elliptic-curve private key encoded in the Base64 ASN.1 byte format</param>

        public void AddPrivateKey(string privateKeyBase64) =>
            AddPrivateKey(Base64.Decode(privateKeyBase64));

        /// <summary>
        /// Unseals the given JSON message, performing the necessary signature verification and decryption steps.
        /// </summary>
        /// <param name="sealedMessage">A message generated by the Google Pay API (in JSON format)</param>
        /// <returns>Unsealed message (in JSON format)</returns>
        public string Unseal(string sealedMessage)
        {
            if (_privateKeys.Count == 0)
            {
                throw new InvalidOperationException("At least one private key must be added");
            }
            PaymentData paymentData = Util.Json.Parse<PaymentData>(sealedMessage);
            SignedMessage signedMessage = Util.Json.Parse<SignedMessage>(paymentData.SignedMessage);
            KeyDerivation keyDerivation;

            switch (paymentData.ProtocolVersion)
            {
                case ECv1:
                    keyDerivation = new KeyDerivation(SymmetricKeySizeECv1, MacKeySizeECv1);
                    break;
                case ECv2:
                    keyDerivation = new KeyDerivation(SymmetricKeySizeECv2, MacKeySizeECv2);
                    break;
                default:
                    throw new SecurityException($"Unsupported protocol version {paymentData.ProtocolVersion}");
            }

            if (!_signatureVerification.VerifyMessage(paymentData, GoogleSenderId, _recipientId, _signatureKeyProvider))
            {
                throw new SecurityException("Cannot verify signature");
            }

            byte[] message = Base64.Decode(signedMessage.EncryptedMessage);
            KeyDerivation.DerivedKeys keys = _privateKeys.Select(key =>
                keyDerivation.Derive(key, signedMessage.EphemeralPublicKey)
            ).FirstOrDefault(derivedKeys =>
                new TagVerification(derivedKeys.MacKey).Verify(message, signedMessage.Tag)
            );

            if (keys == null)
            {
                throw new SecurityException("Cannot decrypt; invalid MAC");
            }
            var decryption = new MessageDecryption(keys.SymmetricEncryptionKey);
            return decryption.Decrypt(message);
        }
    }
}