using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Toggl_API.APIHelper.ClassModel
{
    public class ProjectColor
    {
        private static Random rnd = new Random();
        public string Name { get; set;}
        public int ID { get; set; }
        public System.Windows.Media.Color Color { get; set; }

        public ProjectColor(string name,int id)
        {
            Name = name;
            ID = id;
            this.Color = System.Windows.Media.Color.FromArgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
        }

    
        public ProjectColor(string name, int id, Color color)
        {
            Name = name;
            ID = id;
            this.Color = color;
           
        }

        public ProjectColor()
        {

        }

        /// <summary>
        /// Method for getting color in System.Drawing.Color format
        /// </summary>
        /// <returns>System.Drawing.Color</returns>
        public System.Drawing.Color GetCurrentColor()
        {

            return System.Drawing.Color.FromArgb(this.Color.A, this.Color.R, this.Color.G, this.Color.B);
        }




    }
}
