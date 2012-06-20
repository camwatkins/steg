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
    public partial class Authentication : Form
    {
        /*General collection of userid/corresponding passwords*/
        Dictionary<string, string> passwords = new Dictionary<string, string>();
        Steg steg_interface;   //Steganography interface

        public Authentication()
        {
            InitializeComponent();


            /*Replaces all characters entered with * in tbox_pass*/
            tbox_pass.PasswordChar = '*';
           
            /*Population of passwords dictionary collection*/
            passwords.Add("admin", "admin");
            passwords.Add("jshoben", "stegasaurus");
            passwords.Add("mmorse", "stegaraptor");
            passwords.Add("jyoung", "stegabeast");
            passwords.Add("cwatkins", "stegasteg");
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            login();
        }

        private void login()
        {
            /*userid set to id entered, after setting to lowercase*/
            string userid = tbox_userid.Text.ToLower();

            /*Tests to see userid is valid and password for said userid is also valid before 
            instantiating the steg_interface*/
            if (passwords.ContainsKey(userid) && tbox_pass.Text == passwords[userid])
            {
                Menu menu = new Menu();
                menu.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("User ID and/or Password is incorrect. Please try again.", "Incorrect Authentication!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                tbox_userid.Text = "";
                tbox_pass.Text = "";

                return;
            }
        }

        private void tbox_pass_KeyDown(object sender, KeyEventArgs e)
        {
             if (e.KeyCode == Keys.Enter)
                login(); 
        }

        private void Authentication_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        } 
    }
}
