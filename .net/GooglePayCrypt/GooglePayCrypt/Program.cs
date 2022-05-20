using System;
using GooglePay.PaymentDataCryptography;
using GooglePay.PaymentDataCryptography.Models;
using Newtonsoft.Json;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var keyProvider = new GoogleKeyProvider(true);
            var parser = new PaymentMethodTokenRecipient("merchant:12345678901234567890", keyProvider);

            parser.AddPrivateKey(
                "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgldJlb2plUhI+TRTkNJLJ+oZkeihiEvx3Umoo1hSRKtShRANCAAS/y4dOjfL/A7nNHcqZEVgZF8/pUhIxQOzOuxtwFoUqZx2Dnj5UCZvu475p6i9XMHEJDfCntDUBkMm+AOJLqjSz");

            var paymentData = new PaymentData()
            {
                Signature =
                    "MEUCIFCR9vdNY3IIYBb8mRAbnso2sieGln44IKWCznmi1aBOAiEA6OX/+1ZJBen0wStRVVxe54TQGKaymNQFlrAgaQ7b4to\u003d",
                ProtocolVersion = "ECv2",
                IntermediateSigningKey = new SigningKey()
                {
                    SignedKey =
                        "{\"keyValue\":\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEAyKEEI9KE0EVEWZ/YEJKfp5YZ6AwwRCLF9r66bYLriGHg/OPFN66ksMtYf4HGBxRKiPqDihNKga1cespQS/wVg\\u003d\\u003d\",\"keyExpiration\":\"1653734772240\"}",
                    Signatures = new[]
                    {
                        "MEQCIHX1DoTO5e1j+r0/VIjEgzQjPdu8Qduy+Q1TZayLMGYaAiA6ECTW+v3kJY7BEPG7xA5OonFb7u7HYSIXfM9NMZF7FQ\u003d\u003d"
                    }
                },
                SignedMessage =
                    "{\"encryptedMessage\":\"yHwX37w/1XOhXbRiLJF0briEl62cIDziP8WhrNL2FFD5SGANSWFBOwxMKEzwUFv+YBwclahEX/dyM7RJnjn1Xe+G6qlJuInjMPt38v6dh8XiGqYANFufAg7yxlrzTYnpe4aMzb8BBlyntsw+YVEuHj53dX4Q47omtSAkBU3K/HqJsIDWrcXRpfM6ZHkXACRErzhdPmPtysQKKKdNSmB+mSFr4M8FJCSVB0DucHAZp/Eye0ILdgi49+w6Y1jbxIj+VhULz/a9VQ4UFtwDTCNdlNPJ6Yc8OIwO8hgLcOrSZV6Y+pTrPGnPOF22Jn7eGfMYekkrRyMzrdMKWJQZTqlNS0XHs2WvY2imwtQCGMYsXztou2fKBGQp+ZZumYaG1a+TgMyVlvCdxeMJ9DO5iRJB8/KsRJ1/7vAfqszJd935G695cbM8BaqK7dEGwLM\\u003d\",\"ephemeralPublicKey\":\"BHRQ5hGbNOfQ1eJDgoEf6ZPtJAVC5grcl8axsNIlmG0nxSBNW+fTz/bfCnGYLstcM/6T6hPIWp3KexGKSu2N+ac\\u003d\",\"tag\":\"drNradbW1z2SLaQpKVHGze7zNg/TBH0caXtYv+MnSTw\\u003d\"}"
            };

            var json = JsonConvert.SerializeObject(paymentData);
            var decryptedMessage = parser.Unseal(json);
            Console.WriteLine(decryptedMessage);
            Console.ReadLine();
        }
    }
}