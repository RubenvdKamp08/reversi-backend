using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiBackEnd.Models
{
    public class Spel
    {
        [Key]
        public int GameId { get; set; }
        public string Omschrijving { get; set; }
        public string Token { get; set; }
        public string spelerWitToken { get; set; }
        public string spelerZwartToken { get; set; }
        public string bord { get; set; }
        [NotMapped]
        public int[,] bordArray
        {
            get
            {
                if (bord == null)
                {
                    return null;
                }
                else
                {
                    return JsonConvert.DeserializeObject<int[,]>(this.bord);
                }
            }
            set
            {
                bordArray = value;
            }
        }
        public int aanDeBeurt { get; set; }

        [NotMapped]
        private int[,] TempBoard { get; set; }


        public static string CreateBoard()
        {
            int[,] board = new int[8, 8]
            {
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,1,2,0,0,0 },
                {0,0,0,2,1,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
                {0,0,0,0,0,0,0,0 },
            };
            return JsonConvert.SerializeObject(board);
        }

        public static string CreateGameToken()
        {
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return token.Replace(@"/", "");
        }

        public void SetTurn(Zet zet)
        {
            var board = JsonConvert.DeserializeObject<int[,]>(this.bord);
            this.TempBoard = board;
            if (this.CheckAround(zet.X, zet.Y) == true)
            {
                this.TempBoard[zet.X, zet.Y] = this.aanDeBeurt;

                CheckToFlip(zet.X, zet.Y);
                this.bord = JsonConvert.SerializeObject(this.TempBoard);
                if (CanTurn())
                {
                    GiveTurn();
                }
            }
        }

        private bool CheckAround(int row, int col)
        {
            int ToCheck;
            if (this.aanDeBeurt == 1)
            {
                ToCheck = 2;
            }
            else
            {
                ToCheck = 1;
            }
            var board = this.TempBoard;

            //loop through the rows
            for (int rowDir = -1; rowDir <= +1; rowDir++)
            {
                for (int colDir = -1; colDir <= +1; colDir++)
                {
                    if (rowDir == 0 && colDir == 0)
                    {
                        continue;
                    }

                    int rowCheck = row + rowDir;
                    int colCheck = col + colDir;

                    bool ItemFound = false;

                    if (this.IsValidPosition(rowCheck, colCheck) == true)
                    {
                        if (board[rowCheck, colCheck] == ToCheck)
                        {
                            rowCheck += rowDir;
                            colCheck += colDir;
                            ItemFound = true;
                        }
                    }

                    if (ItemFound)
                    {
                        if (this.IsValidPosition(rowCheck, colCheck) && board[rowCheck, colCheck] == this.aanDeBeurt)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool IsValidPosition(int row, int col)
        {
            return (row >= 0 && row <= 7) && (col >= 0 && col <= 7);
        }

        private void CheckToFlip(int row, int col)
        {
            List<Zet> finalItems = new List<Zet>();
            int ToCheck;
            if (this.aanDeBeurt == 1)
            {
                ToCheck = 2;
            }
            else
            {
                ToCheck = 1;
            }

            for (int rowDir = -1; rowDir <= +1; rowDir++)
            {
                for (int colDir = -1; colDir <= +1; colDir++)
                {
                    if (rowDir == 0 && colDir == 0)
                    {
                        continue;
                    }
                    int rowCheck = row + rowDir;
                    int colCheck = col + colDir;

                    List<Zet> possibleItems = new List<Zet>();

                    if (this.IsValidPosition(rowCheck, colCheck) == true && this.TempBoard[rowCheck, colCheck] == ToCheck)
                    {
                        possibleItems.Add(new Zet()
                        {
                            X = rowCheck,
                            Y = colCheck
                        });

                        rowCheck += rowDir;
                        colCheck += colDir;
                    }
                    if (possibleItems.Count() > 0)
                    {

                        if (this.IsValidPosition(rowCheck, colCheck) && this.TempBoard[rowCheck, colCheck] == this.aanDeBeurt)
                        {
                            finalItems.Add(new Zet()
                            {
                                Y = colCheck,
                                X = rowCheck
                            });

                            foreach (var item in possibleItems)
                            {
                                finalItems.Add(item);
                            }
                        }
                    }
                }
            }
            if (finalItems.Count() > 0)
            {
                foreach (var item in finalItems)
                {
                    this.Flip(item);
                }
            }
        }
        private void Flip(Zet zet)
        {
            if (IsValidPosition(zet.X, zet.Y))
            {

                this.TempBoard[zet.X,zet.Y] = this.aanDeBeurt;
            }
        }

        private bool CanTurn()
        {
            for (int row = 0; row <= 7; row++)
            {
                for (int col = 0; col <= 7; col++)
                {
                    if (CheckAround(row, col))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void GiveTurn()
        {
            if (this.aanDeBeurt == 1)
            {
                this.aanDeBeurt = 2;
            }
            else
            {
                this.aanDeBeurt = 1;
            }
        }

    }
}
