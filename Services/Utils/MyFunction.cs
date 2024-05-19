using System.Drawing;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using BarcodeLib;
using Data.Entities;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Utils
{
    public static class MyFunction
    {
        public static string GetId(this ClaimsPrincipal user)
        {
            var idClaim = user.Claims.FirstOrDefault(i => i.Type.Equals("UserId"));
            if (idClaim != null)
            {
                return idClaim.Value;
            }
            return "";
        }

        public static async Task<Token> GetAccessToken(User user, List<string> role, IConfiguration _configuration)
        {
            List<Claim> claims = GetClaims(user, role);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddHours(int.Parse(_configuration["Jwt:ExpireTimes"])),
              //int.Parse(_configuration["Jwt:ExpireTimes"]) * 3600
              signingCredentials: creds);

            var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return new Token
            {
                Access_token = serializedToken,
                TokenType = "Bearer",
                ExpiresIn = int.Parse(_configuration["Jwt:ExpireTimes"]!) * 3600,
                UserID = user.Id.ToString(),
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber!,
                Email = user.Email!,
                Avatar = user.Avatar!,
                Name = user.Name!,
                Roles = role
            };
        }

        public static List<Claim> GetClaims(User user, List<string> roles)
        {
            IdentityOptions _options = new IdentityOptions();
            var claims = new List<Claim> {
                new Claim("UserId", user.Id.ToString()),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            if (!string.IsNullOrEmpty(user.PhoneNumber)) claims.Add(new Claim("PhoneNumber", user.PhoneNumber));
            if (!string.IsNullOrEmpty(user.Email)) claims.Add(new Claim("Email", user.Email));
            if (!string.IsNullOrEmpty(user.UserName)) claims.Add(new Claim("UserName", user.UserName));

            return claims;
        }

        public static string ConvertToUnSign(string input)
        {
            input = input.Trim();
            for (int i = 0x20; i < 0x30; i++)
            {
                input = input.Replace(((char)i).ToString(), " ");
            }
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string str = input.Normalize(NormalizationForm.FormD);
            string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
            while (str2.IndexOf("?") >= 0)
            {
                str2 = str2.Remove(str2.IndexOf("?"), 1);
            }
            return str2;
        }

        public static TimeSpan TimespanCalculate(DateTime dateTime)
        {
            return dateTime - DateTime.Now;
        }

        public static async Task<string> UploadImageAsync(IFormFile file, string path)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var extension = Path.GetExtension(file.FileName);

            var imageName = DateTime.Now.ToBinary() + Path.GetFileName(file.FileName);

            string filePath = Path.Combine(path, imageName);

            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filePath.Split("/app/wwwroot")[1];
        }

        public static async Task<string> UploadFileAsync(IFormFile file, string path, string splitString)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var extension = Path.GetExtension(file.FileName);

            var imageName = DateTime.Now.ToBinary() + Path.GetFileName(file.FileName);

            string filePath = Path.Combine(path, imageName);

            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            var test = filePath.Split(splitString);

            return test[1];
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static async Task<FileEModel> DownloadFile(string filePath)
        {
            var result = new FileEModel();
            result.Content = File.ReadAllBytes(filePath);
            result.Extension = Path.GetExtension(filePath);
            return result;
        }



        public static (int position, DateTime[] days) GetWeekInfo()
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;

            var days = new DateTime[7];
            for (int i = 0; i < 7; i++)
            {
                days[i] = today.AddDays(-dayOfWeek + i);
            }

            return (dayOfWeek, days);
        }

        public static List<DateTime> Get7DaysWithToday()
        {
            var today = DateTime.Today;
            var dates = new List<DateTime>();

            for (int i = 0; i < 7; i++)
            {
                dates.Insert(0, today.AddDays(-i));
            }

            return dates;
        }

        public static byte[] GenerateBarcode(string content, BarcodeLib.TYPE barcodeType = BarcodeLib.TYPE.CODE128)
        {
            Barcode barcode = new BarcodeLib.Barcode()
            {
                // Use the minimum bar width of 1 pixel. Setting this causes
                // BarcodeLib to ignore the Width property and create the minimum-width
                // barcode.
                BarWidth = 1,
            };
            barcode.IncludeLabel = true;
            barcode.Alignment = AlignmentPositions.CENTER;
            int minWidth = Math.Max(100, barcode.EncodedValue.Length);
            //Image barcodeImage = barcode.Encode(BarcodeLib.TYPE.CODE128, content, minWidth, 200);
            using (MemoryStream stream = new MemoryStream())
            {

                //barcodeImage.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static byte[] GenerateQrcode(string content)
        {
            byte[] QRCode = null;
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            QRCodeData dataQr = qRCodeGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode bitmap = new BitmapByteQRCode(dataQr);
            QRCode = bitmap.GetGraphic(20);
            return QRCode;
        }

        static int IndexOf(byte[] source, byte[] target)
        {
            int len = source.Length - target.Length + 1;

            for (int i = 0; i < len; i++)
            {
                bool match = true;

                for (int j = 0; j < target.Length; j++)
                {
                    if (source[i + j] != target[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        public static byte[] ReplaceBytes(byte[] source, byte[] oldBytes, byte[] newBytes)
        {
            int index = IndexOf(source, oldBytes);

            if (index != -1)
            {
                // Tạo mảng byte mới với văn bản đã thay thế
                byte[] result = new byte[source.Length - oldBytes.Length + newBytes.Length];

                // Copy các phần tử từ mảng byte nguồn đến vị trí cần thay thế
                Buffer.BlockCopy(source, 0, result, 0, index);

                // Copy văn bản mới vào mảng byte kết quả
                Buffer.BlockCopy(newBytes, 0, result, index, newBytes.Length);

                // Copy các phần tử từ mảng byte nguồn sau vị trí cần thay thế
                Buffer.BlockCopy(source, index + oldBytes.Length, result, index + newBytes.Length, source.Length - (index + oldBytes.Length));

                return result;
            }

            // Trường hợp không có văn bản cần thay thế
            return source;
        }
    }
}

