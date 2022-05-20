from google_pay_token_decryption import GooglePayTokenDecryptor

root_signing_keys = [{
    "keyValue": "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEGnJ7Yo1sX9b4kr4Aa5uq58JRQfzD8bIJXw7WXaap/hVE+PnFxvjx4nVxt79SdRuUVeu++HZD0cGAv4IOznc96w==",
    "protocolVersion": "ECv2",
    "keyExpiration": "2154841200000"
}]
recipient_id = "merchant:12345678901234567890"
private_key = "MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgldJlb2plUhI+TRTkNJLJ+oZkeihiEvx3Umoo1hSRKtShRANCAAS/y4dOjfL/A7nNHcqZEVgZF8/pUhIxQOzOuxtwFoUqZx2Dnj5UCZvu475p6i9XMHEJDfCntDUBkMm+AOJLqjSz"

decryptor = GooglePayTokenDecryptor(root_signing_keys, recipient_id, private_key)

encrypted_token = {
    "signature": "MEQCIHOqGZqQgZbg2v0/H4OYzH5u/Wz4t6B1cLLc2SN6a36AAiBshQIhPqwdc+t3SVdocX8MJEZVnuHEucweDZU3I8a2wg\u003d\u003d",
    "intermediateSigningKey": {
        "signedKey": "{\"keyValue\":\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEDkC87ijxyPOkxx6nj5FhaEH2IcyW2z1xEk4Ku9tCXBLi8fYFwcxP8A1RqzZV5ZMsiTMYZ2uV8xaRDNnQjsh06g\\u003d\\u003d\",\"keyExpiration\":\"1653715443593\"}",
        "signatures": [
            "MEUCIQDIKrlp5NIu6XzpOW6RrC16UVTXyXD6kFg4JpdPGnSNtwIgMYGko6MK7GJsQ7NJ4Ix/lwlX6WHtAfTQzOUDLazxEo0\u003d"]},
    "protocolVersion": "ECv2",
    "signedMessage": "{\"encryptedMessage\":\"qAw+xkT8BLmz7LoNwZsem+R4eJnF1mbz4fYw3Y2HBcSNKXao4SPHxJzEcFG2bAkBHj1rHIanoZd/gH7vgwLIT/rIxGKxdvVezHvkkbyP7YtKmbS0qz3gokvlJvjkHpJOkeiHZChOhqRCQR4DaLZ6C92NSdQGoqKuzxUh58+QFt/5hcXHY1yZ2VxDwfyDlCNKCMlYSbGTFNekG+FkxEQE9qrr2o+raCha5zEgprciuPFpbUPK5LTuuavlgSWxLIcbqDDKhF5A09vyECf1UI2tu+5xfDVMh1aAG/Z7net4aMHGLr5oXqlxduJ9xbP88fBdZ8ZtAmYQH01AuIv7y2GpKT5mRp/nf047NZWT+MKqpeK1bGdDETEQgsz8BLUvuxzf9Z4cVOk7fJ9RkPialcHC+ZvqIDWsurAwPzE96znxLeP1pxRauh/MLApHmFA\\u003d\",\"ephemeralPublicKey\":\"BIQri5nIl/PQ9fiw+8JvlWBk3u2XsMWXmIdWQUjI0JgyydBbg/THFq1VwKG1I8dtuwbFSjA4NSBiSjRoP/1Vi7M\\u003d\",\"tag\":\"Z1LPzydxevR38FXHFqbGb91fNmjXimH2LDIj6a/WYdA\\u003d\"}"}

decrypted_token = decryptor.decrypt_token(encrypted_token)
print(decrypted_token)
