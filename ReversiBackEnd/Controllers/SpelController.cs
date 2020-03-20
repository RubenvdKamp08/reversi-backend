using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReversiBackEnd.DAL;
using ReversiBackEnd.Models;

namespace ReversiBackEnd.Controllers
{
    [Route("api/Spel")]
    [ApiController]
    public class SpelController : ControllerBase
    {
        private readonly UserContext _userContext;        

        public SpelController(UserContext userContext)
        {
            _userContext = userContext;
        }
        
        // GET api/Spel/token
        [HttpGet("{token}")]
        public ActionResult<Spel> Get(string token)
        {           
            var result = _userContext.Spels.FirstOrDefault(item => item.Token == token);

            if (result != null) return result;
            else return new Spel { Token = "0", Omschrijving = "0", spelerWitToken = "0", spelerZwartToken = "0" };
        }
        
        [HttpGet("beurt/{token}")]
        public ActionResult<int> GetBeurt(string token)
        {            
            var result = _userContext.Spels.FirstOrDefault(item => item.Token == token);
            if (result != null)
            {
                return result.aanDeBeurt;
            } else
            {
                return 0;
            }
        }        
        
        // PUT api/values/5
        [HttpPut("zet/{id:int}")]
        public ActionResult<Spel> Put([FromBody] Zet zet)
        {            
            var result = _userContext.Spels.FirstOrDefault(item => item.Token == zet.SpelToken);
            if (result != null)
            {
                if (zet.SpelerToken == result.spelerWitToken)
                {
                    result.SetTurn(zet);
                    result.aanDeBeurt = 2;
                    _userContext.SaveChanges();
                }
                else if (zet.SpelerToken == result.spelerZwartToken)
                {
                    result.SetTurn(zet);
                    result.aanDeBeurt = 1;
                    _userContext.SaveChanges();
                } else
                {
                    result.aanDeBeurt = 0;
                    _userContext.SaveChanges();
                }
                return result;
            }
            else
            {
                return new Spel { Token = "0", Omschrijving = "0", spelerWitToken = "0", spelerZwartToken = "0" };
            }
        }
        
        [HttpPost("createGame")]
        public ActionResult<Spel> Post(Spel spel)
        {            
            string description = spel.Omschrijving;
            string whiteToken = spel.spelerWitToken;
            string board = Spel.CreateBoard();
            string token = Spel.CreateGameToken();
            Spel addSpel = new Spel { Omschrijving = description, spelerWitToken = whiteToken, spelerZwartToken = "", bord = board, Token = token, aanDeBeurt = 1 };
            _userContext.Add(addSpel);
            _userContext.SaveChanges();
            return addSpel;
        }

        [HttpGet("joinable")]
        public ActionResult<IEnumerable<JoinableGame>> Get()
        {
            var result = _userContext.Spels.Where(item => item.spelerZwartToken == "").Select(item => new JoinableGame { Omschrijving = item.Omschrijving, Token = item.Token}).ToList();
            return result;
        }

        [HttpPost("joinJoinable")]
        public ActionResult<Spel> Post(SecondPlayer secondPlayer)
        {
            var gameToUpdate = _userContext.Spels.SingleOrDefault(s => s.Token == secondPlayer.Token);
            if(gameToUpdate != null)
            {
                gameToUpdate.spelerZwartToken = secondPlayer.spelerZwartToken;
                _userContext.SaveChanges();
            }
            return gameToUpdate;
        }

        [HttpGet("statusGame/{token}")]
        public ActionResult<int> GetStatus(string token)
        {
            //0 -> nog geen spel
            //1 -> spel is bezig
            //2 -> wachten op tegenstander
            var result = _userContext.Spels.SingleOrDefault(item => item.spelerWitToken == token && item.spelerZwartToken != "");
            if (result != null)
            {
                return 1;
            }
            result = _userContext.Spels.FirstOrDefault(item => item.spelerZwartToken == token);
            if (result != null)
            {
                return 1;
            }
            result = _userContext.Spels.FirstOrDefault(item => item.spelerWitToken == token && item.spelerZwartToken == "");
            if (result != null)
            {
                return 2;
            }
            return 0;

        }        
    }
}