/*STEGANOGRAPHY PROJECT
 CIS4364.901S12 CRYPTOLOGY AND INFO SECURITY
 DESIGNED BY mm, js, cw, jy*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace steg_proj
{
    public partial class Menu : Form
    {

        Steg steg_interface;
        embedded embed_interface;

        public Menu()
        {
            InitializeComponent();
        }

        /* choose to inject text into a cover BMP */
        private void button1_Click(object sender, EventArgs e)
        {
            steg_interface = new Steg();
            steg_interface.Show();         
        }

        /* choose to embed an encrypted into a cover */
        private void button2_Click(object sender, EventArgs e)
        {
            embed_interface = new embedded();
            embed_interface.Show();          
        }

        /* closes application */
        private void Menu_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        /* return to login */
        private void button3_Click(object sender, EventArgs e)
        {
            Authentication login = new Authentication();
            login.Show();
            this.Hide();
        }
    }
}
