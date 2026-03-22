using System.Security.Cryptography;

public class AuthService
{
    public bool VerifyPassword(string loginPassword, string storedHash)
    {
        var hashParts = storedHash.Split('.'); // Descomponer el hash almacenado. Se usa split para obtener List<string> con dos elementos: [<salt_base64>, <hash_base64>].

        if (hashParts.Length != 2) { return false; } // En caso de no existir solo dos partes, significa que el hash está corrupto.

        var salt = Convert.FromBase64String(hashParts[0]); // Decodificación del Salt.
        var hash = Convert.FromBase64String(hashParts[1]); // Decodificación del Hash.

        var loginHash = Rfc2898DeriveBytes.Pbkdf2(loginPassword, salt, 100_000, HashAlgorithmName.SHA256, 32); // A partir de la Salt recuperada, generar otro hash con el password proporcionado para iniciar sesión.

        return CryptographicOperations.FixedTimeEquals(hash, loginHash); // Realizar una comparación segura: true = son idénticos | false = son diferentes.
    }
}