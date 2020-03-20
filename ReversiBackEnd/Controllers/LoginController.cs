using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ReversiBackEnd.DAL;
using ReversiBackEnd.Extensions;
using ReversiBackEnd.Models;

namespace ReversiBackEnd.Controllers
{
    public class LoginController : Controller
    {


        UserManager _userManager = new UserManager();

        [HttpGet, ActionName("Index")]
        [Route("login")]
        public IActionResult Index()
        {
            return View(new Player());
        }

        [HttpPost, ActionName("Index")]
        [Route("login")]
        public async Task<IActionResult> Index([Bind("InputName", "Password")] Player user)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    Player ingelogd = await _userManager.SignIn(HttpContext, user);
                    if (ingelogd.Token != null)
                    {
                        HttpContext.Session.Set<Player>(Globals.SessionKeyGame, ingelogd);
                        return RedirectToAction(nameof(HomePage));
                    } else
                    {
                        ModelState.AddModelError("IncorectLogin", "wachtwoord of gebruikersnaam is incorrect");
                        return View(user);
                    }
                } catch (Exception ex)
                {
                    ModelState.AddModelError("summary", ex.Message);
                    return View(user);
                }
            } else
            {
                return View(user);
            }
        }

        [HttpGet, ActionName("Register")]
        [Route("login/register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost, ActionName("Register")]
        [Route("login/register")]
        public async Task<IActionResult> Register([Bind("Email", "UserName", "Password")] Player player)
        {
            if (ModelState.IsValid)
            {
                Player p = new Player();
                p.Email = player.Email;
                p.UserName = player.UserName;
                p.Role = "Player";

                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: player.Password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                p.Salt = salt;
                p.Password = hashed;
                string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                token.Replace(@"/", "");

                p.Token = token;

                using (SqlConnection sqlCon = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Reversi; Integrated Security=True;"))
                {
                    string query = "INSERT INTO Users VALUES(@UserName, @Email, @Password, @Salt, @Role, @Token)";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@UserName", p.UserName);
                    sqlCmd.Parameters.AddWithValue("@Email", p.Email);
                    sqlCmd.Parameters.AddWithValue("@Password", p.Password);
                    sqlCmd.Parameters.AddWithValue("@Salt", p.Salt);
                    sqlCmd.Parameters.AddWithValue("@Role", p.Role);
                    sqlCmd.Parameters.AddWithValue("@Token", p.Token);

                    sqlCon.Open();
                    sqlCmd.ExecuteNonQuery();
                    sqlCon.Close();
                }
                return (RedirectToAction(nameof(Index)));
            } else
            {
                return View();
            }
        }

        [Authorize(Roles = "Player")]
        [HttpGet, ActionName("homepage")]
        [Route("homepage")]
        public IActionResult HomePage()
        {
            Player p = HttpContext.Session.Get<Player>(Globals.SessionKeyGame);
            int status = 0;
            int flag;
            List<JoinableGame> JoinableGames = new List<JoinableGame>();
            ViewData["UserId"] = p.UserId;
            ViewData["Token"] = p.Token;
            using (var client1 = new HttpClient())
            {
                client1.BaseAddress = new Uri("https://localhost:44373");
                var responseTask1 = client1.GetAsync("api/Spel/statusGame/" + p.Token);
                responseTask1.Wait();

                var result1 = responseTask1.Result;
                if(result1.IsSuccessStatusCode)
                {
                    var readTask1 = result1.Content.ReadAsStringAsync();
                    readTask1.Wait();
                    status = Convert.ToInt32(readTask1.Result);
                }
                if (status == 1)
                {
                    return RedirectToAction("Joined");
                }
                else if (status == 2)
                {
                    return RedirectToAction("Waiting");
                }
                else
                {

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://localhost:44373/");
                        var responseTask = client.GetAsync("api/Spel/joinable");
                        responseTask.Wait();

                        var result = responseTask.Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var readTask = result.Content.ReadAsAsync<IList<JoinableGame>>();
                            readTask.Wait();
                            JoinableGames = readTask.Result.ToList();
                        }
                    }
                }
            }
            ViewData["JoinableGames"] = JoinableGames;
            return View(JoinableGames);
        }  
        
        
        [Authorize(Roles = "Player")]
        [HttpGet, ActionName("joinJoinable")]
        public IActionResult joinJoinable(string token)
        {
            Player p = HttpContext.Session.Get<Player>(Globals.SessionKeyGame);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44373/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = client.PostAsJsonAsync("api/Spel/joinJoinable", new SecondPlayer
                {
                    Token = token,
                    spelerZwartToken = p.Token
                }).Result;
                if(result.IsSuccessStatusCode)
                {
                    return RedirectToAction("homepage");
                }
            }
            return View();
        } 
        
        
        [Authorize(Roles = "Player")]
        [HttpGet, ActionName("createGame")]
        [Route("createGame")]
        public async Task<IActionResult> CreateGame()
        {            
            return View();
        }

        
        [Authorize(Roles = "Player")]
        [HttpPost, ActionName("createGame")]
        [Route("createGame")]
        public async Task<IActionResult> CreateGame([Bind("omschrijving")]string omschrijving)
        {
            Player p = HttpContext.Session.Get<Player>(Globals.SessionKeyGame);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44373/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = client.PostAsJsonAsync("api/Spel/createGame", new GameAdd
                {
                    Omschrijving = omschrijving,                    
                    spelerWitToken = p.Token
                }).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("waiting");
                }
            }
                                
           
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator");
            return View();
        }


        [Authorize(Roles ="Player")]
        [HttpGet, ActionName("waiting")]
        [Route("waiting")]
        public IActionResult Waiting()
        {
            return View();
        }
        

        [Authorize(Roles ="Player")]
        [HttpGet, ActionName("joined")]
        [Route("joined")]
        public IActionResult Joined()
        {
            return Redirect("/");
        }
















        /*
        private readonly UserContext _context;
        
        public LoginController(UserContext context)
        {
            _context = context;
        }

        // GET: Login
        public async Task<IActionResult> Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Index([Bind("InputName, Password")] Player player)
        {
            if(PlayerExists(player.InputName))
            {
                if (_context.Players.Any(e => e.Name == player.InputName))
                {
                    var salt = _context.Players.FirstOrDefault(item => item.Name == player.InputName).Salt;
                    var inputhash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: player.Password,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8));
                    if (_context.Players.Any(e => e.Password == inputhash))
                    {
                        Player p = await _context.Players.FirstOrDefaultAsync(m => m.Name == player.InputName);
                        HttpContext.Session.Set<Player>(Globals.SessionKeyGame, p);
                        return RedirectToAction(nameof(HomePage));
                    }
                } else if (_context.Players.Any(e => e.Email ==player.InputName && e.Password == player.Password)) {
                    var salt = _context.Players.FirstOrDefault(item => item.Email == player.InputName).Salt;
                    var inputhash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: player.Password,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8));
                    if (_context.Players.Any(e => e.Password == inputhash))
                    {
                        Player p = await _context.Players.FirstOrDefaultAsync(m => m.Name == player.InputName);
                        HttpContext.Session.Set<Player>(Globals.SessionKeyGame, p);
                        return RedirectToAction(nameof(HomePage));
                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> HomePage()
        {
            Player p = HttpContext.Session.Get<Player>(Globals.SessionKeyGame);
            return View(p);
        }
        

        // GET: Login/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Login/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Name,Password")] Player player)
        {
            if (ModelState.IsValid)
            {
                if (_context.Players.Any(item => item.Email == player.Email))
                {
                }
                else
                {
                    byte[] salt = new byte[128 / 8];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(salt);
                    }

                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: player.Password,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8));


                    var jwtTokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = jwtTokenHandler.CreateJwtSecurityToken();
                    var token = jwtTokenHandler.WriteToken(jwtToken);

                    Player p = new Player { Email = player.Email, Name = player.Name, Password = hashed, Salt = salt, Token = token };
                    _context.Add(p);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(player);
 
        }


        private bool PlayerExists(string Name)
        {
            return _context.Players.Any(e => e.Name == Name || e.Email == Name);
        } */
    }
}
