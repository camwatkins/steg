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
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
namespace steg_proj
{
    public partial class embedded : Form
    {
        /* assosciated list of formats that the embedder has been tested with */
        string filter = "AVI (*.AVI)|*.avi|BMP (*.BMP)|*.bmp|GIF (*.GIF)|*.gif|JPEG (*.JPG)|*.jpg|PNG (*.PNG)|*.png|";
        string filterCont = "MPEG (*.MPG)|*.mpg|MP3 (*.MP3)|*.mp3|RAR (*.RAR)|(*.rar)|TXT (*.TXT)|*.txt|WAV (*.WAV)|*.wav|ZIP (*.ZIP)|*.zip";
       
        public embedded()
        {
            InitializeComponent();
        }

        /* select cover file*/
        private void button1_Click(object sender, EventArgs e)
        {
            /*This code opens up an "Open File" dialog to set the pathname of the image that we will use to conceal the secret message entered*/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string filename = null;

            string newFilt = filter + filterCont;

            openFileDialog1.Filter = newFilt; 

            DialogResult result = openFileDialog1.ShowDialog();

            //if user presses cancel while in "Open File" dialog, method is terminated and control is returned to user
            if (result == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                tbox_image.Text = filename;
            }

            else
            {
                return;
            }
        }

        /* select user's text file */
        private void button2_Click(object sender, EventArgs e)
        {
            tbox_msg.Text = "";

            /*This code opens up an "Open File" dialog to set the pathname of the image that we will use to conceal the secret message entered*/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string filename = null;

            openFileDialog1.Filter = "Text Files(*.TXT)|*.txt";

            DialogResult result = openFileDialog1.ShowDialog();

            //if user presses cancel while in "Open File" dialog, method is terminated and control is returned to user
            if (result == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                tbox_textfile.Text = filename;

                string[] txt_read = System.IO.File.ReadAllLines(filename);

                StreamReader stream = new StreamReader(filename);
                string line = stream.ReadLine();

                while (line != null)
                {
                    tbox_msg.AppendText(line);
                    line = stream.ReadLine();
                }

                stream.Close();
            }

            else
            {
                return;
            }
        }

        /* resets all encryption related fields */
        private void button3_Click(object sender, EventArgs e)
        {
            tbox_image.Text = "";
            tbox_textfile.Text = "";
            tbox_msg.Text = "";
            tbox_msg.Enabled = true;
        }

        /* start the embedding process */
        private void button4_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            embed();
            this.Cursor = Cursors.Default;
        }

        /* unembed the selected file */
        private void button7_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            unembed();
            this.Cursor = Cursors.Default;
        }

        /* clear unembed fields */
        private void button5_Click(object sender, EventArgs e)
        {
            tbox_unembed.Text = "";
            tbox_result.Text = "";
            tbox_result.Enabled = true;
        }

        /* select file to be unembedded */
        private void button6_Click(object sender, EventArgs e)
        {
            /*This code opens up an "Open File" dialog to set the pathname of the image that we will use to conceal the secret message entered*/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string filename = null;

            string newFilt = filter + filterCont;
            openFileDialog1.Filter = newFilt;

            DialogResult result = openFileDialog1.ShowDialog();

            //if user presses cancel while in "Open File" dialog, method is terminated and control is returned to user
            if (result == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                tbox_unembed.Text = filename;
            }
            else
                return;
        }

        private void embed()
        {
            if (tbox_image.Text.Equals(""))
            {
                MessageBox.Show("Steganography aborted. Filepath is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (tbox_textfile.Text.Equals("") && tbox_msg.Text.Equals(""))
            {
                MessageBox.Show("Steganography aborted. Filepath is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string coverFile = tbox_image.Text;

            string text = tbox_msg.Text;

            /* encrypting user's plaintext x5 */
            text = encrypt(text);
            text = encrypt(text);
            text = encrypt(text);
            text = encrypt(text);
            text = encrypt(text);

            /* mark indicates the binary split point, this string should be considered unique corresponding 
               to individual pieces of software */
            string mark = "[5PL17!"; 

            string pathname;

            /* gathering file extension information for later use */
            FileInfo fInfoCover = new FileInfo(tbox_image.Text);

            /* if user has selected to upload a text file */
            if (!(tbox_textfile.Text.Equals("")))
            {
                FileInfo fInfoText = new FileInfo(tbox_textfile.Text);
                pathname = tbox_textfile.Text.Remove(tbox_textfile.Text.Length - 4) + "_TEMP" + fInfoText.Extension;
                mark += text;

                /* writing temporary file as backup */
                System.IO.File.WriteAllText(pathname, mark);
            }

            /* user has entered text into the box */
            else
            {
                pathname = tbox_image.Text.Remove(tbox_image.Text.Length - 4) + "_TEMP" + ".txt";
                mark += text;

                /* writing temporary file as backup */
                System.IO.File.WriteAllText(pathname, mark);

            }

            /*taking in the bytes of both the cover and plaintext files */
            byte[] coverArray = System.IO.File.ReadAllBytes(coverFile);
            byte[] textArray = System.IO.File.ReadAllBytes(pathname);

            /* merging the two byte arrays */
            byte[] combo = new byte[coverArray.Length + textArray.Length];
            Buffer.BlockCopy(coverArray, 0, combo, 0, coverArray.Length);
            Buffer.BlockCopy(textArray, 0, combo, coverArray.Length, textArray.Length);

            /* writing the new file with correct format extension */
            File.WriteAllBytes(tbox_image.Text.Remove(tbox_image.Text.Length - 4) + "_NEW" + fInfoCover.Extension, combo);

            /* deleting the temp file */
            File.Delete(pathname);

            MessageBox.Show("Encryption and Steganography successful!\r", "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void unembed()
        {

            tbox_result.Text = "";
            if (tbox_unembed.Text.Equals(""))
            {
                MessageBox.Show("Steganography aborted. Filepath is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /* intializing variables */

            string path = tbox_unembed.Text;
            byte[] hidden = System.IO.File.ReadAllBytes(path);
            string s = System.Text.Encoding.UTF8.GetString(hidden);

            int i = (s.IndexOf("[5PL17!"));
            
            /* verifying that the file has actually been encrypted */
            if (i == -1)
            {
                MessageBox.Show("No steganography detected in file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            /* seperating the cover from the embedded file */
            string[] delim = new string[] { "[5PL17!" };
            string[] code = s.Split(delim, StringSplitOptions.None);
            string clipped = code.Last();
          
            string result = clipped;

            /* decrypting x5 */
            result = decrypt(result, result.Length);
            result = decrypt(result, result.Length);
            result = decrypt(result, result.Length);
            result = decrypt(result, result.Length);
            result = decrypt(result, result.Length);

            /* sends decrypted plain text to a .txt file if it is two large for tbox_result */ 
            if (result.Length < 100)
            {
                tbox_result.Text = result;
                MessageBox.Show("Steganography extraction successful!", "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                DialogResult dr;

                sfd.FileName = "Steganograph_Revealed_Text.txt";

                dr = MessageBox.Show("Steganography extraction complete!\r\nExtracted message is too large to be shown; select a file to save output to.",
                    "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (dr == System.Windows.Forms.DialogResult.OK)
                    DialogResult = sfd.ShowDialog();

                if (DialogResult == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, result);
                }
                else
                {
                    MessageBox.Show("Steganography extraction aborted. No file selected.", "Operation Failure", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private string encrypt(string Plain_Text)
        {
            try
            {
                string message = Plain_Text;
                int length = message.Length;

                if (length % 5 != 0)
                {
                    int remainder = ((5 - (length % 5)));
                    for (int i = 0; i < remainder; i++)
                    {
                        message += " ";
                    }
                    length = message.Length;
                }

                int[] messageHolder = new int[length];
                for (int i = 0; i < length; i++)
                {
                    // The range we are using for the Unicode is 10 to 126. Subtracting 10 to take care of the offset 
                    // This gives us 116 different characters to work with
                    messageHolder[i] = ((System.Convert.ToInt32(message[i]) - 10));
                    messageHolder[i] = ((messageHolder[i] + 42) % 116);
                }

                int arrayLength = length;
                bool transpose = true;
                int start = 0;
                int finish = arrayLength;
                int temp;

                while (transpose)
                {
                    for (int i = start, j = (finish - 5); i < (start + 5); i++, j++)
                    {
                        temp = messageHolder[i];
                        messageHolder[i] = messageHolder[j];
                        messageHolder[j] = temp;
                    }
                    start += 5;
                    finish -= 5;
                    if (start == finish || start > finish)
                    {
                        transpose = false;
                    }
                }
                message = "";

                for (int i = 0; i < length; i++)
                {
                    // Adding 32 to fix the offset used earlier
                    message += System.Convert.ToChar((messageHolder[i]) + 10);
                }

                return message;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private string decrypt(string Cipher_Text, int Cipher_Length)
        {
            try
            {

                int length = Cipher_Length;
                int arrayLength = length;
                bool transpose = true;
                int start = 0;
                int finish = arrayLength;
                int temp;
                int[] messageHolder = new int[length];
                string message = Cipher_Text;

                for (int i = 0; i < length; i++)
                {
                    // The range we are using for the Unicode is 110 to 126. Subtracting 10 to take care of the offset 
                    // This gives us 94 different characters to work with
                    messageHolder[i] = ((System.Convert.ToInt32(message[i]) - 10));
                }

                while (transpose)
                {
                    for (int i = start, j = (finish - 5); i < (start + 5); i++, j++)
                    {
                        temp = messageHolder[j];
                        messageHolder[j] = messageHolder[i];
                        messageHolder[i] = temp;
                    }
                    start += 5;
                    finish -= 5;
                    if (start == finish || start > finish)
                    {
                        transpose = false;
                    }
                }

                for (int i = 0; i < length; i++)
                {
                    // The range we are using for the Unicode is 10 to 126. Subtracting 32 to take care of the offset 
                    // This gives us 94 different characters to work with
                    messageHolder[i] = ((116 + (messageHolder[i] - 42)) % 116);
                }

                message = "";

                for (int i = 0; i < length; i++)
                {
                    // Adding 32 to fix the offset used earlier
                    message += System.Convert.ToChar((messageHolder[i]) + 10);
                }

                return message;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

    }
}
