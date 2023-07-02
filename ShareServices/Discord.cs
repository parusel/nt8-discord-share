#region Using declarations
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
#endregion

//This namespace holds Share adapters in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.ShareServices
{
	public class Discord : ShareService
	{
		#region Class Var		

		private object icon;
		#endregion

		public override void CopyTo(NinjaScript ninjaScript)
		{
			base.CopyTo(ninjaScript);

			PropertyInfo[] props = ninjaScript.GetType().GetProperties();
			foreach (PropertyInfo pi in props)
			{
				if (pi.Name == "Webhook Url")
					pi.SetValue(ninjaScript, _webhookUrl);				
			}
		}
		
		protected override void OnStateChange()
		{			
			if (State == State.SetDefaults)
			{
				Description	= @"Service for sending information to Discord";
				Name 						= "Discord";
				CharacterLimit				= 2000;
				UseOAuth = false;
				IsConfigured = true;
				IsDefault = true;
				Signature = string.Empty;
				IsImageAttachmentSupported = true;
				_webhookUrl = ""; // User has to enter webhook url
			}
			else if (State == State.Configure)
			{
				CharactersReservedPerMedia = 0;
				IsImageAttachmentSupported = true;
			}
		}

		public override async Task OnShare(string text, string imgFilePath)
		{
			SendMessageAndFile( _webhookUrl, text, imgFilePath);
		}
		
		public void ApplyPreconfiguredSettings(string name)
		{
			if (name == "Discord")
			{
				_webhookUrl = "";
			}
		}

		static void SendMessageAndFile( string url, string message, string filePath)
		{
			HttpClient client = new HttpClient();

			MultipartFormDataContent content = new MultipartFormDataContent();

			if( ! string.IsNullOrEmpty(filePath))
			{
				var file = File.ReadAllBytes(filePath);

				content.Add( new ByteArrayContent( file, 0, file.Length ), Path.GetExtension( filePath ), filePath );
			}

			content.Add(new StringContent(message),"content");

			client.PostAsync( url, content ).Wait();

			client.Dispose();
		}
	
		public override object Icon
		{
			get
			{
				// Instantiate a Grid on which to place the image
				Grid myCanvas = new Grid { Height = 16, Width = 16 };

				BitmapImage iconBitmapImage = GetDiscordLogo();

				// Instantiate an Image to place on the Grid
				Image image = new Image
				{
					Height = 16,
					Width = 16,
					Source = iconBitmapImage
				};

				// Add the image to the Grid
				myCanvas.Children.Add( image );

				return myCanvas;
			}
		}

		public BitmapImage GetDiscordLogo()
		{
			var webClient = new WebClient();
			var imageBytes = webClient.DownloadData("https://discordapp.com/assets/2c21aeda16de354ba5334551a883b481.png");
			var stream = new MemoryStream(imageBytes);

			// Create a new BitmapImage
			BitmapImage image = new BitmapImage();

			// Set the image's source to the stream
			image.BeginInit();
			image.StreamSource = stream;
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.EndInit();
			image.Freeze();

			// Return the image
			return image;
		}

		#region Properties
		
		[Display(Name = "Webhook Url", GroupName = "Discord Parameters", Order = 0)]
		public string _webhookUrl { get; set; }
		
		#endregion
	}
}
