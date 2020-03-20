using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using ReversiBackEnd.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReversiBackEnd.DAL
{
    public class UserManager
    {
        private const string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Reversi; Integrated Security=True;";

        public async Task<Player> SignIn(HttpContext httpContext, Player user, bool isPersistent = false)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                bool ingelogd = false;
                Player playerOutput = new Player();
                string firstQuery = "SELECT Salt FROM Users WHERE Email = @InputName or UserName = @InputName";

                sqlCon.Open();
                SqlCommand sqlCmd = new SqlCommand(firstQuery, sqlCon);
                sqlCmd.Parameters.AddWithValue("@InputName", user.InputName);
                SqlDataReader rdr = sqlCmd.ExecuteReader(CommandBehavior.SingleRow);
                byte[] salt = new byte[128 / 8];
                string test; ;

                if (rdr.Read())
                {                  
                    salt = (byte[])rdr["Salt"];
                    test = rdr["Salt"].ToString();
                }

                sqlCon.Close();


                string queryString = "SELECT UserId, UserName, Email, Password, Role, Token FROM Users WHERE ( Email= @InputName or UserName = @InputName ) and Password=@Password";


                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: user.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));


                sqlCon.Open();
                SqlCommand sqlCmd1 = new SqlCommand(queryString, sqlCon);
                sqlCmd1.Parameters.AddWithValue("@InputName", user.InputName);
                sqlCmd1.Parameters.AddWithValue("@Password", hashed);
                SqlDataReader rdr1 = sqlCmd1.ExecuteReader(CommandBehavior.SingleRow);
                var player = new Player();

                if (rdr1.Read())
                {
                    playerOutput.UserId = Convert.ToInt32(rdr1["UserId"]);
                    player.UserId = Convert.ToInt32(rdr1["UserId"]);
                    player.UserName = rdr1["UserName"].ToString();
                    //player.Password = rdr1["Password"].ToString();
                    player.Role = rdr1["Role"].ToString();
                    player.Token = rdr1["Token"].ToString();
                    playerOutput.Token = rdr1["Token"].ToString();
                    ingelogd = true;
                }

                sqlCon.Close();
                if (ingelogd == true)
                {

                    ClaimsIdentity identity = new ClaimsIdentity(this.GetUserClaims(player), CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }
                return playerOutput;
            }
        }

        public static bool CheckEmail(Player user)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                bool Exist = false;

                string queryString = "SELECT Email FROM Users WHERE Email = @Email";

                sqlCon.Open();
                SqlCommand sqlCmd = new SqlCommand(queryString, sqlCon);
                sqlCmd.Parameters.AddWithValue("@Email", user.Email);
                SqlDataReader rdr = sqlCmd.ExecuteReader(CommandBehavior.SingleRow);
                if(rdr.Read())
                {
                    Exist = true;
                }
                sqlCon.Close();
                return Exist;
            }
        }

        public static bool CheckUserName(Player user)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                bool Exist = false;

                string queryString = "SELECT UserName FROM Users WHERE UserName = @UserName";

                sqlCon.Open();
                SqlCommand sqlCmd = new SqlCommand(queryString, sqlCon);
                sqlCmd.Parameters.AddWithValue("@UserName", user.UserName);
                SqlDataReader rdr = sqlCmd.ExecuteReader(CommandBehavior.SingleRow);
                if (rdr.Read())
                {
                    Exist = true;
                }
                sqlCon.Close();
                return Exist;
            }
        }

        private IEnumerable<Claim> GetUserClaims(Player user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }

        private IEnumerable<Claim> GetUserRoleClaims(Player user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
            return claims;
        }

        public async void SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync();
        }        
    }
}
