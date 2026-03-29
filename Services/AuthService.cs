/*********************************************************************************************************************
 *     █████╗ ██╗     ███████╗██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██████╗  ██████╗ ███████╗    ███████╗███╗   ███╗     *
 *    ██╔══██╗██║     ██╔════╝╚██╗██╔╝██╔══██╗████╗  ██║██╔══██╗██╔══██╗██╔═══██╗██╔════╝    ██╔════╝████╗ ████║     *
 *    ███████║██║     █████╗   ╚███╔╝ ███████║██╔██╗ ██║██║  ██║██████╔╝██║   ██║███████╗    █████╗  ██╔████╔██║     *
 *    ██╔══██║██║     ██╔══╝   ██╔██╗ ██╔══██║██║╚██╗██║██║  ██║██╔══██╗██║   ██║╚════██║    ██╔══╝  ██║╚██╔╝██║     *
 *    ██║  ██║███████╗███████╗██╔╝ ██╗██║  ██║██║ ╚████║██████╔╝██║  ██║╚██████╔╝███████║    ███████╗██║ ╚═╝ ██║     *
 *    ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝    ╚══════╝╚═╝     ╚═╝     *
 *                                                                                                                   *
 *                                                                                                                   *
 *                                 Copyright (c) 2026 Sinuhé Alejandro Gómez Hernández                               *
 *                                                                                                                   *
 *                              Permission is granted for free use, but NOT for sale/rent.                           *
 *                             Commercial use is prohibited without explicit authorization.                          *
 *                                                                                                                   *
 *********************************************************************************************************************/
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

    public string PasswordHasher(string newPassword)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16); // 6 bytes de datos aleatorios que hacen único a cada hash, incluso de contraseñas identicas.
        var hashedPassword = Rfc2898DeriveBytes.Pbkdf2(
            newPassword,                //Contraseña proporcionada por el usuario.
            salt,                       // Sal generada de manera aleatoria.
            100_000,                    // Número de iteraciones.
            HashAlgorithmName.SHA256,   // Algoritmo de cifrado.
            32                          // Tamaño de la salida en 32 bytes.
        );

        string composited_hash = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hashedPassword)}"; // Cadena combinada de <salt_base64>.<hash_base64>
        return composited_hash;
    }
}