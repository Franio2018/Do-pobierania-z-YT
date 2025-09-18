using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDownloadLib
{
    public class MusicListManagment
    {
        public void WriteListOfSongs(List<(string Title, string Url)> ListaUtworow)
        {
            if (ListaUtworow.Count <= 0)
            {
                throw new Exception("Lista nie zawiera żadnych piosenek!");
            }
            Console.WriteLine("Piosenki z playlisty:");
            int i = 0;
            foreach (var item in ListaUtworow)
            {
                Console.WriteLine($"{i++}: " + item.Title + " " + item.Url);
            }
        }
    }
}
