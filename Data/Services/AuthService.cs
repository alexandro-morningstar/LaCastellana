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
        // === Descomponer el hash almacenado. Se usa split para obtener List<string> con dos elementos: [<salt_base64>, <hash_base64>].
        var hashParts = storedHash.Split('.');

        // === En caso de no existir exactamente dos partes, significa que el hash está corrupto.
        if (hashParts.Length != 2) { return false; }

        var salt = Convert.FromBase64String(hashParts[0]); // Decodificación del Salt.
        var hash = Convert.FromBase64String(hashParts[1]); // Decofificación del Hash.

        // === A partir de la Salt recuperada, generar otro hash con el password proporcionado por el usuario para iniciar sesión.
        var loginHash = Rfc2898DeriveBytes.Pbkdf2(loginPassword, salt, 100_000, HashAlgorithmName.SHA256, 32);

        // === Realizar una comparación segura: true = son idénticos | false = son diferentes.
        return CryptographicOperations.FixedTimeEquals(hash, loginHash);
    }

    
}