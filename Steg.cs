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
    public partial class Steg : Form
    {
        string binary_result = null;    /*Bit string extracted from picture*/
        string result = null;    /*Revealed message after being converted back to ASCII text*/

        string conceal_message = null;
        string conceal_binary = null;     /*Concealed message converted to binary from ASCII*/
        
        public Steg()
        {
            InitializeComponent();
        }

        private void Steg_FormClosed(object sender, FormClosedEventArgs e)
        {
   
        }

        private void btn_upload_Click(object sender, EventArgs e)
        {
            /*This code opens up an "Open File" dialog to set the pathname of the image that we will use to conceal the secret message entered*/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string filename = null;

            openFileDialog1.Filter = "Image Files(*.BMP)|*.bmp";

            DialogResult result = openFileDialog1.ShowDialog();

            //if user presses cancel while in "Open File" dialog, method is terminated and control is returned to user
            if (result == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                tbox_image.Text = filename;
            }
            else
                return;
        }

        private void btn_upload_txt_Click(object sender, EventArgs e)
        {
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
                return;
        }

        private void btn_confirm_Click(object sender, EventArgs e)
        {
            if (tbox_msg.Text != null && tbox_msg.Text.Length > 0)
            {
                this.Cursor = Cursors.WaitCursor;
                conceal();
                this.Cursor = Cursors.Default;
            
            }
            else
            {
                MessageBox.Show("Enter a message to be concealed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            tbox_image.Text = "";
            tbox_textfile.Text = "";
            tbox_msg.Text = "";
            tbox_msg.Enabled = true;
        }

        private void btn_upload2_Click(object sender, EventArgs e)
        {
            /*This code opens up an "Open File" dialog to set the pathname of the image that we will use to conceal the secret message entered*/
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string filename = null;

            openFileDialog1.Filter = "Image Files(*.BMP)|*.bmp";

            DialogResult result = openFileDialog1.ShowDialog();

            //if user presses cancel while in "Open File" dialog, method is terminated and control is returned to user
            if (result == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                tbox_image2.Text = filename;
            }
            else
                return;
        }

        private void btn_reveal_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            reveal();
            this.Cursor = Cursors.Default;
        }

        private void tbox_msg_KeyDown(object sender, KeyEventArgs e)
        {
            /* if (e.KeyCode == Keys.Enter)
             {
                 if (tbox_msg.Text != null && tbox_msg.Text.Length > 0)
                     conceal();
                 else
                     MessageBox.Show("Enter a message to be concealed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
             }*/
        }

        private void conceal()
        {
            Color pixel; //pixel object used to point to whichever pixels we choose to edit in the uploaded picture
            Bitmap image = null; //bitmap object used to bring the image into the program via filepath selected
            StreamReader sr;

            if (tbox_msg.Enabled == false)
            {
                if (!tbox_textfile.Text.Substring(tbox_textfile.Text.Length - 4).Equals(".txt"))
                {
                    MessageBox.Show("Steganography aborted. File is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    sr = new StreamReader(tbox_textfile.Text);
                    conceal_message = sr.ReadToEnd();
                    sr.Close();
                }
                catch
                {
                    MessageBox.Show("Steganography aborted. File is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            
            else
            {
                conceal_message = tbox_msg.Text;   //message to be concealed set to text in messagebox
            }

            /*Max text length is 2^16, since only 16 bits are set aside as a header to indicate 
             original message length for decryption*/
            if (conceal_message.Length > 65536)
            {
                MessageBox.Show("Steganography aborted. Input is too large.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /* message entered is encrypted into ciphertext x5 */
            conceal_message = encrypt(conceal_message);
            conceal_message = encrypt(conceal_message);
            conceal_message = encrypt(conceal_message);
            conceal_message = encrypt(conceal_message);
            conceal_message = encrypt(conceal_message);


            if (conceal_message == null)
            {
                MessageBox.Show("Encryption failed. Contact Administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int message_length = conceal_message.Length;

            string filename = tbox_image.Text; //sets filename variable to pathname of uploaded image
            try
            {

                image = new Bitmap(filename);  //attempts to initialize bitmap with file at specified path; if the file isn't a bmp or 
                //is corrupt, terminates this method and returns control to user
            }
            catch
            {
                MessageBox.Show("Steganography aborted. File is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /*Width and Height of picture in pixels*/
            int Width = image.Width;
            int Height = image.Height;

            /*pixel coordinates for embedding encrypted message in image. Begins at (6, 0) since the first 
             six pixels are the message length header*/
            int width_incr = 6;
            int height_incr = 0;

            /*byte values used to create a new pixel to be inserted back into the image*/
            byte red_byte = 0;
            byte blue_byte = 0;
            byte green_byte = 0;

            /*Message length converted to binary, then padded to length 16 bits*/
            string message_length_binary = Convert.ToString(message_length, 2);
            int leftover_bits = (16 - message_length_binary.Length); 

            for (int p = 0; p < leftover_bits; p++)
            {
                message_length_binary = message_length_binary.Insert(0, "0");
            }

            /* First 16 pixels along the x-coordinates are filled with bits from message_length_binary, 
               in order to pass along the message length when decrypting */

            int iterator = 0;

            for (int x = 0; x <= 5; x++)
            {
                pixel = image.GetPixel(x, 0);

                /*3 byte local variables are created from sending the bytes representing each color (Red, green, and blue) of the currently 
                 * selected pixel through LSB_substitution. This method will be where the LSB of the bytes passed in as parameters is replaced with 
                 a bit from the secret message entered*/
                red_byte = LSB_substitution(pixel.R, message_length_binary[iterator].ToString());
                iterator++;

                /*On the sixth iteration, the red byte will contain the 16th bit of message length, thus
                 does not need to advance afterwards*/
                if (iterator < 16)
                {
                    green_byte = LSB_substitution(pixel.G, message_length_binary[iterator].ToString());
                    iterator++;
                    blue_byte = LSB_substitution(pixel.B, message_length_binary[iterator].ToString());
                    iterator++;
                }

                /*New pixel is created from the color values above, which are set from original pixel's 
                 modified color byte values*/
                pixel = Color.FromArgb(pixel.A, red_byte, green_byte, blue_byte);

                /*Attempts to set new pixel back into image at the original coordinates*/
                try
                {
                    image.SetPixel(x, 0, pixel);
                }

                catch
                {
                    MessageBox.Show("Steganography aborted. Only 24bit BMPs are supported.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            /*Converts message to binary*/
            Convert_To_Binary(conceal_message);

            for (int r = 0; r < conceal_binary.Length; r++)
            {

                /*If end of width is reached, x is set back to 0 and y is incremented*/
                if (width_incr >= Width)
                {
                    width_incr = 0;
                    height_incr++;
                }


                /*coordinates for pixel to be fetched from image*/
                int x = width_incr;
                int y = height_incr;

                pixel = image.GetPixel(x, y);

                /*3 byte local variables are created from sending the bytes representing each color (Red, green, and blue) of the currently 
                 * selected pixel through LSB_substitution. This method will be where the LSB of the bytes passed in as parameters is replaced with 
                 a bit from the secret message entered*/
                if (r < conceal_binary.Length)
                {
                    red_byte = LSB_substitution(pixel.R, conceal_binary[r].ToString());
                    r++;
                }

                if (r < conceal_binary.Length)
                {
                    green_byte = LSB_substitution(pixel.G, conceal_binary[r].ToString());
                    r++;
                }

                if (r < conceal_binary.Length)
                {
                    blue_byte = LSB_substitution(pixel.B, conceal_binary[r].ToString());
                }

                /*pixel (color object) is redirected to a new color created from the four adjusted color bytes created above using Color.FromArgB*/
                pixel = Color.FromArgb(pixel.A, red_byte, green_byte, blue_byte);

                /*The new color that pixel is set to is reinserted into the picture at the same coordinates generated above*/
                image.SetPixel(x, y, pixel);

                /*Test Pixel*/
                pixel = image.GetPixel(x, y);

                width_incr++;
            }
            
            /*After all bits of the secret message have been placed into the image, this method saves the modified image under a new filename 
             in the same directory.*/
            string pathname = tbox_image.Text.Remove(tbox_image.Text.Length - 4) + "-NEW";
            int version = 0;
            if (!File.Exists(pathname + ".bmp"))
            {
                pathname = pathname + ".bmp";
            }
            else
            {
                while (true)
                {
                    if (File.Exists(pathname + version.ToString() + ".bmp"))
                    {
                        version++;
                    }
                    else
                    {
                        pathname = pathname + version.ToString() + ".bmp";
                        break;
                    }

                }

            }

            /*Modified image is saved to pathname created above*/
            image.Save(pathname, System.Drawing.Imaging.ImageFormat.Bmp);

            /*Dispose bitmap object when finished to deallocate its memory*/
            image.Dispose();

            tbox_msg.Text = "";
            tbox_image.Text = "";

            MessageBox.Show("Encryption and Steganography successful!\r\nModified image is located in " + pathname, "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void reveal()
        {
            Color pixel; //pixel object used to point to whichever pixels we choose to edit in the uploaded picture
            Bitmap image = null; //bitmap object used to bring the image into the program via filepath selected
            int message_length = 0;
            binary_result = "";
            result = "";

            string filename = tbox_image2.Text; //sets filename variable to pathname of uploaded image
            try
            {
                image = new Bitmap(filename);  //attempts to initialize bitmap with file at specified path; if the file isn't a bmp or 
                //is corrupt, terminates this method and returns control to user
            }
            catch
            {
                MessageBox.Show("Steganography aborted. File is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int Width = image.Width;
            int Height = image.Height;

            /*Need at least 16 pixels for the possibility of a message length header*/
            if ((Width * Height) < 16)
            {
                MessageBox.Show("Steganography aborted. File is too small.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string length_binary = "";

            /*String together the LSB of each byte from the first 16 pixels*/
            for (int x = 0; x <= 5; x++)
            {
                pixel = image.GetPixel(x, 0);
                length_binary += LSB_retrieve(pixel.R);

                /*terminate on sixth runthrough, since the red byte will be the 16th bit of the header*/
                if (x != 5)
                {
                    length_binary += LSB_retrieve(pixel.G);
                    length_binary += LSB_retrieve(pixel.B);
                }
            }

            /*Set message_length to the binary string above converted into an int*/
            message_length = Convert.ToInt32(length_binary, 2);


            /*Total number of pixels needs to be less than the bits of message_length divided by 3, 
             * since each pixel can fit 3 bits*/
            if ((Width * Height) < (message_length / 3))
            {
                MessageBox.Show("Steganography aborted. File is too small.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            /*pixel coordinates for extracting encrypted message from image. Begins at (6, 0) since the first 
             six pixels are the message length header*/
            int width_incr = 6;
            int height_incr = 0;

            /*binary message length of embedded cipher text */
            int message_binary_length = message_length * 8;

            for (int t = 0; t < message_binary_length; t++)
            {
                /*Increments y when end of width is reached, sets x back to 0*/
                if (width_incr >= Width)
                {
                    width_incr = 0;
                    height_incr++;
                }

                /*Coordinates of pixel to be fetched*/
                int x = width_incr;
                int y = height_incr;

                pixel = image.GetPixel(x, y); //Points pixel (color object) to the pixel at the specified coordinates in the picture

                /*3 byte local variables are created from sending the bytes representing each color (Alpha, red, green, and blue) of the currently 
                 * selected pixel through LSB_substitution. This method will be where the LSB of the bytes passed in as parameters is replaced with 
                 a bit from the secret message entered*/

                /*Terminates decryption if t is greater than 250000, as this indicates no sign of 
                 steganography implanted*/
                if (!(t < 250000))
                {
                    MessageBox.Show("No steganography detected in file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (t < message_binary_length)
                {
                    binary_result += LSB_retrieve(pixel.R);
                    t++;
                }
                if (t < message_binary_length)
                {
                    binary_result += LSB_retrieve(pixel.G);
                    t++;
                }
                if (t < message_binary_length)
                {
                    binary_result += LSB_retrieve(pixel.B);
                }

                width_incr++;
            }

            /*Iterates through the bit string extracted, taking 8 characters at a time and converting 
             them to their corresponding ASCII character, which is then appended to result, the string 
             variable holding the extracted ciphertext*/
            for (int m = 0; m < message_length; m++)
            {
                string bit_string = null;

                for (int n = 0; n < 8; n++)
                {
                    bit_string += binary_result[(n + (m * 8))];
                }

                char character = Convert.ToChar(Convert.ToInt32(bit_string, 2));
                result += character;
            }

            /*Once the ciphertext has been built, result is decrypted into its original plaintext*/
            result = decrypt(result, message_length);
            result = decrypt(result, message_length);
            result = decrypt(result, message_length);
            result = decrypt(result, message_length);
            result = decrypt(result, message_length);

            /*If result is less than 100 characters (simple message), it will be displayed in the 
             Reveal textbox. If more, prompts user to save output to a textfile.*/
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


        private void Convert_To_Binary(string Message)
        {
            /*Converts each character of encrypted concealed message to ASCII value, 
             then to padded 8-bit string appended to conceal_binary*/
            for (int i = 0; i < conceal_message.Length; i++)
            {
                int temp_int = Convert.ToInt32(conceal_message[i]);

                string buffer = Convert.ToString(temp_int, 2);
                int leftover_bits = 8 - buffer.Length;

                for (int x = 0; x < leftover_bits; x++)
                {
                    buffer = buffer.Insert(0, "0");
                }

                conceal_binary += buffer;
            }
        }

        private byte LSB_substitution(byte Orig_Color, string Insert_Bit)
        {
            //bit_buffer is a string representation of the 8 bits of Orig_Color byte, padded to be 8 bits long
            string bit_buffer = Convert.ToString(Orig_Color, 2);

            int leftover_bits = 8 - bit_buffer.Length;

            for (int x = 0; x < leftover_bits; x++)
            {
                bit_buffer = bit_buffer.Insert(0, "0");
            }

            /*bit buffer removes the first character of itself (the "LSB") and inserts a bit from the concealed message, passed 
             through the parameter "Insert_Bit"*/
            bit_buffer = (bit_buffer.Remove(0, 1)).Insert(0, Insert_Bit);

            /*Modified bit string is converted back into a new byte, which is returned to the caller*/
            byte new_byte = Convert.ToByte(bit_buffer, 2);

            return new_byte;
        }

        private string LSB_retrieve(byte Modified_Color)
        {
            /*bit buffer is bit-string representation of Modified_Color byte, padded to be 8 bits long*/
            string bit_buffer = Convert.ToString(Modified_Color, 2);

            int leftover_bits = 8 - bit_buffer.Length;

            for (int x = 0; x < leftover_bits; x++)
            {
                bit_buffer = bit_buffer.Insert(0, "0");
            }

            /*Extracts first bit of string (the least significant bit) and returns bit*/
            string bit = bit_buffer[0].ToString();

            return bit;
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
