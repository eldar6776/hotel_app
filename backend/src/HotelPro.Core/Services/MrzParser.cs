using System.Text.RegularExpressions;

namespace HotelPro.Core.Services;

public class MrzParser
{
    public static MrzResult? ParseTd3(string line1, string line2)
    {
        if (line1.Length != 44 || line2.Length != 44) return null;

        try
        {
            var type = line1[0..1];
            var issuingCountry = line1[2..5];
            var names = line1[5..44].Split("<<", 2, StringSplitOptions.None);
            var lastName = names[0].Replace("<", " ").Trim();
            var firstName = names.Length > 1 ? names[1].Replace("<", " ").Trim() : "";

            var docNumber = line2[0..9].Replace("<", "");
            var nationality = line2[10..13];
            var dobStr = line2[13..19];
            var gender = line2[20..21];
            var expiryStr = line2[21..27];
            var checksum = line2[27..28];

            var dob = ParseMrzDate(dobStr);
            var expiry = ParseMrzDate(expiryStr);

            return new MrzResult
            {
                FirstName = firstName,
                LastName = lastName,
                DocumentNumber = docNumber,
                Nationality = nationality,
                DateOfBirth = dob,
                Gender = gender == "M" ? "M" : gender == "F" ? "F" : null,
                ExpiryDate = expiry,
                DocumentType = "Passport",
                IsExpired = expiry.HasValue && expiry.Value < DateTime.UtcNow,
                IsOver18 = dob.HasValue && dob.Value <= DateTime.UtcNow.AddYears(-18)
            };
        }
        catch
        {
            return null;
        }
    }

    public static MrzResult? ParseTd1(string line1, string line2, string line3)
    {
        if (line1.Length != 30 || line2.Length != 30 || line3.Length != 30) return null;

        try
        {
            var docNumber = line2[0..9].Replace("<", "");
            var nationality = line2[15..18];
            var dobStr = line2[0..6];
            var gender = line2[7..8];
            var expiryStr = line2[8..14];
            var names = line3[0..30].Split("<<", 2, StringSplitOptions.None);
            var lastName = names[0].Replace("<", " ").Trim();
            var firstName = names.Length > 1 ? names[1].Replace("<", " ").Trim() : "";

            // Offsets corrected for TD1
            var dobOff = 0; var expOff = 8; var natOff = 15;
            dobStr = line2.Substring(dobOff, 6);
            expiryStr = line2.Substring(expOff, 6);
            nationality = line2.Substring(natOff, 3);

            var dob = ParseMrzDate(dobStr);
            var expiry = ParseMrzDate(expiryStr);

            return new MrzResult
            {
                FirstName = firstName,
                LastName = lastName,
                DocumentNumber = docNumber,
                Nationality = nationality,
                DateOfBirth = dob,
                Gender = gender == "M" ? "M" : gender == "F" ? "F" : null,
                ExpiryDate = expiry,
                DocumentType = "IDCard",
                IsExpired = expiry.HasValue && expiry.Value < DateTime.UtcNow,
                IsOver18 = dob.HasValue && dob.Value <= DateTime.UtcNow.AddYears(-18)
            };
        }
        catch
        {
            return null;
        }
    }

    public static MrzResult? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = Regex.Replace(text, "\r\n|\r", "\n");
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 2 && lines[0].Length == 44)
            return ParseTd3(lines[0], lines[1]);
        if (lines.Length == 3 && lines[0].Length == 30)
            return ParseTd1(lines[0], lines[1], lines[2]);

        return null;
    }

    private static DateTime? ParseMrzDate(string yyMMdd)
    {
        if (yyMMdd.Length != 6) return null;
        if (!int.TryParse(yyMMdd[0..2], out var yy)) return null;
        if (!int.TryParse(yyMMdd[2..4], out var mm)) return null;
        if (!int.TryParse(yyMMdd[4..6], out var dd)) return null;
        var year = yy > 50 ? 1900 + yy : 2000 + yy;
        if (mm < 1 || mm > 12 || dd < 1 || dd > 31) return null;
        return new DateTime(year, mm, dd);
    }
}

public class MrzResult
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string DocumentNumber { get; set; } = "";
    public string Nationality { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string DocumentType { get; set; } = "";
    public bool IsExpired { get; set; }
    public bool IsOver18 { get; set; }
}
