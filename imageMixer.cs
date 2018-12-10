using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ImageMixer
{
   	public partial class ImageMixer : Form
        {
		PictureBox pb;
		String[] arguments;
	 	volatile float trans_value_1 = 0.5F;
	 	volatile float trans_value_2 = 0.5F;
		Image image1;
		Image image2;
		Image image1_init;
		Image image2_init;
		Bitmap bm1;
		Bitmap bm2;
		Graphics gr1;
		Graphics gr2;
		Image final_img;
		
		Image inverted_output;
		string output_file = @"G:\output.txt";
		int fromClipboard = 0;
		
		[DllImport("kernel32.dll")]
 		 [return: MarshalAs(UnmanagedType.Bool)]
 		 static extern bool AllocConsole();
 		[System.Runtime.InteropServices.DllImport("user32.dll")]
		 private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
		 
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		 private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		Dictionary<string, dynamic> image_types = new Dictionary<string, dynamic>();
		
		enum KeyModifier
		{
			None = 0,
			Alt = 1,
			Control = 2,
			Shift = 4,
			WinKey = 8
		}

		public ImageMixer(String[] args)
		{
			this.arguments = args;
			InitializeComponent();
			
			this.StartPosition = FormStartPosition.CenterScreen;
 			if ((this.arguments.Length == 3) && (this.arguments[2] == "clipboard"))
			{
				this.fromClipboard = 1;
			}
			
			if (this.fromClipboard == 0)
			{
				if (this.arguments.Length <=1)
				{
					AllocConsole();
					Console.WriteLine("Invalid inputs supplied");
					Console.WriteLine("Syntax: " + this.arguments[0] + " <Image 1> <Image 2>");
					Console.Write("Press a key to continue...");
					Console.Read();
					this.Close();
					Application.Exit();
				}

				try
				{
					if (this.arguments.Length < 3)
					{
						AllocConsole();
						Console.WriteLine("Invalid inputs supplied");
						Console.WriteLine("Syntax: " + this.arguments[0] + " <Image 1> <Image 2>");
						Console.Write("Press a key to continue...");
						Console.Read();
						this.Close();
						Application.Exit();
					}
					if (!File.Exists(this.arguments[1]))
					{
						AllocConsole();
						Console.WriteLine("File " + this.arguments[1] + " not found");
						this.Close();
						Application.Exit();
					}
					if (!File.Exists(this.arguments[2]))
					{
						AllocConsole();
						Console.WriteLine("File " + this.arguments[2] + " not found");
						this.Close();
						Application.Exit();
					}

				} catch {
					AllocConsole();
					Console.WriteLine("Invalid inputs supplied");
					Console.WriteLine("Syntax: " + this.arguments[0] + " <Image 1> <Image 2>");
					Console.Write("Press a key to continue...");
					Console.Read();
					this.Close();
					Application.Exit();
				}
				this.image1 = Image.FromFile(this.arguments[1]);
				this.image2 = Image.FromFile(this.arguments[2]);
			}
			else
			{
				this.image1 = Image.FromFile(this.arguments[0].Trim('"'));
				this.image2 = Image.FromFile(this.arguments[1].Trim('"'));
			}		
			this.pb.Width = 500;
			this.pb.Height = 500;
		
			
			this.image1 = ResizeImage(this.image1, this.pb.Width, this.pb.Height);
			this.image2 = ResizeImage(this.image2, this.pb.Width, this.pb.Height);
			
			this.image1 = (Image) ChangeOpacity(this.image1, this.trans_value_1);
			this.image1_init = (Image) ChangeOpacity(this.image1, this.trans_value_1);
			
			this.image2 = (Image) ChangeOpacity(this.image2, this.trans_value_2);
			this.image2_init = (Image) ChangeOpacity(this.image2, this.trans_value_2);
			
			this.bm1 = new Bitmap(this.image1);
			this.bm2 = new Bitmap(this.image2);
			this.gr1 = Graphics.FromImage(this.bm1);
			this.gr2 = Graphics.FromImage(this.bm2);
			
			this.gr1.DrawImage(this.bm2, 0, 0);
			this.final_img = (Image) this.bm1;
			
			this.ClientSize = new System.Drawing.Size(this.pb.Width, this.pb.Height);
			this.FormBorderStyle = FormBorderStyle.None;
			
			this.pb.Image = this.final_img;
			this.pb.SizeMode = PictureBoxSizeMode.Zoom;
			this.pb.BorderStyle = BorderStyle.None;
			this.pb.Show();
			
			this.KeyPreview = true;
			
			this.Show();

			this.inverted_output = InvertImage(this.pb.Image);
			
		}
		
		public static Bitmap ResizeImage(Image image, int width, int height)
		{
		    var destRect = new Rectangle(0, 0, width, height);
		    var destImage = new Bitmap(width, height);

		    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

		    using (var graphics = Graphics.FromImage(destImage))
		    {
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			using (var wrapMode = new ImageAttributes())
			{
			    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
			    graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);
			}
		    }

		    return destImage;
		}
		
		protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
		{
	    		try
	    		{
				if ( (Control.ModifierKeys & Keys.Control) == Keys.Control )
				{
					if (e.Delta > 0)
					{
						this.trans_value_1 = 0.9F;
					}
					else
					{
						this.trans_value_1 = 1.1F;
					}
					if (this.bm1 != null)
						this.bm1.Dispose();
					if (this.bm2 != null)
						this.bm2.Dispose();
					if (this.gr1 != null)
						this.gr1.Dispose();
					if (this.gr2 != null)
						this.gr2.Dispose();


					this.bm1 = new Bitmap(this.image1);
					this.bm2 = new Bitmap(this.image2);
					this.gr1 = Graphics.FromImage(this.bm1);
					this.gr2 = Graphics.FromImage(this.bm2);

					this.image1 = (Image) ChangeOpacity(this.image1, this.trans_value_1);

					this.gr1.DrawImage(this.image2, 0, 0);
					this.final_img = (Image) this.bm1;
					// this.inverted_output = InvertImage(this.final_img);
					this.pb.Image = this.final_img;
					this.pb.Refresh();
				}
				else if ( (Control.ModifierKeys & Keys.Alt) == Keys.Alt )
				{
					if (e.Delta > 0)
					{
						this.trans_value_2 = 0.9F;
					}
					else
					{
						this.trans_value_2 = 1.1F;
					}

					if (this.bm1 != null)
						this.bm1.Dispose();
					if (this.bm2 != null)
						this.bm2.Dispose();
					if (this.gr1 != null)
						this.gr1.Dispose();
					if (this.gr2 != null)
						this.gr2.Dispose();

					this.bm1 = new Bitmap(this.image1);
					this.bm2 = new Bitmap(this.image2);
					this.gr1 = Graphics.FromImage(this.bm1);
					this.gr2 = Graphics.FromImage(this.bm2);

					this.image2 = (Image) ChangeOpacity(this.image2, this.trans_value_2);

					this.gr1.DrawImage(this.image2, 0, 0);
					this.final_img = (Image) this.bm1;
					// this.inverted_output = InvertImage(this.final_img);
					this.pb.Image = this.final_img;
					this.pb.Refresh();
				}			
			}
			catch
			{
				
			}
	    	}
		
		public static Bitmap ChangeOpacity(Image img, float opacityvalue)
		{
	    		Bitmap bmp = new Bitmap(img.Width,img.Height); // Determining Width and Height of Source Image
	    		Graphics graphics = Graphics.FromImage(bmp);
	    		System.Drawing.Imaging.ColorMatrix colormatrix = new System.Drawing.Imaging.ColorMatrix();
	    		colormatrix.Matrix33 = opacityvalue;
	    		System.Drawing.Imaging.ImageAttributes imgAttribute = new System.Drawing.Imaging.ImageAttributes();
	    		imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		   		graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
	    		graphics.Dispose();   // Releasing all resource used by graphics 
	    		return bmp;
		}
		
		private static Image InvertImage(Image originalImg)
		{
		    	Bitmap invertedBmp = null;
		
			using (Bitmap originalBmp = new Bitmap(originalImg))
			{
				invertedBmp = new Bitmap(originalBmp.Width, originalBmp.Height);

				for (int x = 0; x < originalBmp.Width; x++)
				{
					for (int y = 0; y < originalBmp.Height; y++)
					{
						//Get the color
						Color clr = originalBmp.GetPixel(x, y);

						//Invert the clr
						clr = Color.FromArgb(255 - clr.R, 255 - clr.G, 255 - clr.B);

						//Update the color
						invertedBmp.SetPixel(x, y, clr);
					}
				}
			}

			return (Image)invertedBmp;
		}

		public void ImageMixer_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)0x73)
			{
				SaveFileDialog saveFileDialog1 = new SaveFileDialog();      
				saveFileDialog1.InitialDirectory = @"G:\"; 
				saveFileDialog1.Title = "Save File"; 
				saveFileDialog1.CheckFileExists = true; 
				saveFileDialog1.CheckPathExists = true; 
				saveFileDialog1.ShowHelp = true;
				saveFileDialog1.FilterIndex = 2; 
				saveFileDialog1.RestoreDirectory = true;      
				saveFileDialog1.CheckFileExists = false;
				saveFileDialog1.FilterIndex = 1;
				saveFileDialog1.OverwritePrompt = false;
				if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
				{ 
					string [] ext_array = saveFileDialog1.FileName.Split('.');
					string ext = ext_array[ext_array.Length - 1];
					
					this.inverted_output.Save(saveFileDialog1.FileName, this.image_types[ext]);
				}
			}

		}
	
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
		       	switch(keyData)
		       	{
				case Keys.Escape:
					this.Close();
					Application.Exit();
					break;

			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		#region Windows Form Designer generated code
		
		private void InitializeComponent()
		{
			this.pb = new PictureBox();

			this.Controls.Add(this.pb);
			// this.autoscroll=true;
			this.FormBorderStyle = FormBorderStyle.None;

			this.KeyPress += new KeyPressEventHandler(ImageMixer_KeyPress);
			
			this.image_types.Add("png", System.Drawing.Imaging.ImageFormat.Png);
			this.image_types.Add("jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
			this.image_types.Add("bmp", System.Drawing.Imaging.ImageFormat.Bmp);


		}
		
		#endregion
		
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (Clipboard.ContainsText(TextDataFormat.Text))
			{
				string clipboardText = Clipboard.GetText(TextDataFormat.Text);
				try
				{
					string[] filePaths = clipboardText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
					bool is_OK = true;
					foreach (string filePath in filePaths)
					{
						string fps = filePath.Trim('"');
						if(!File.Exists(fps))
						{
							is_OK = false;
							break;
						}
					}
					
					if (is_OK)
					{
						var myList = new List<string>();
					
						myList.Add(filePaths[0]);
						myList.Add(filePaths[1]);
						myList.Add("clipboard");
					
						// Convert to array
						var myArray = myList.ToArray();
						
						Application.Run(new ImageMixer(myArray));
					}
					else
					{
						string[] args = Environment.GetCommandLineArgs();
						Application.Run(new ImageMixer(args));
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
					AllocConsole();
					Console.WriteLine("Invalid inputs supplied");
					Console.WriteLine("Syntax: imageMixer.exe <Image 1> <Image 2>");
					Console.Write("Press a key to continue...");
					Console.Read();
					Application.Exit();
				}				
			}
		}
        }
}