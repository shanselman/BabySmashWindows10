using BabySmash.Core.Services;
using System.Collections.Generic;
using System.Linq;
using BabySmash.Core.Models;
using Windows.Media.SpeechSynthesis;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Globalization;

namespace BabySmash.Windows.Services
{
	public class LanguageService : ILanguageService
	{
		public LanguageService()
		{
			this.synthesizer = new SpeechSynthesizer();
			this.resourceManager = new ResourceManager("BabySmash.Core.Resources.Languages.ResourcesBabySmash", typeof(ILanguageService).GetTypeInfo().Assembly);
			GetDefaultSSML();
		}

		public Language DefaultLanguage
		{
			get
			{
				return this.defaultLanguage ?? (this.defaultLanguage = GetLanguages().FirstOrDefault(v => v.Id == SpeechSynthesizer.DefaultVoice.Id));
			}
		}

		public IList<Language> GetLanguages()
		{
			return this.availableLanguages ?? (this.availableLanguages = SpeechSynthesizer.AllVoices.Select(v => new Language { Id = v.Id, Locale = v.Language, FriendlyName = v.DisplayName }).ToList());
		}

		public string GetLanguageTextForLetter(string letter, string locale)
		{
			return string.Format(defaultSSML, letter, locale);
		}

		public string GetLanguageTextForShape(ShapeType shape)
		{
			var key = shape.ToString().ToLower();
			return GetResourceText(key);
		}

		public string GetResourceText(string key)
		{
			string result = resourceManager.GetString (key, new CultureInfo (DefaultLanguage.Locale));
			return result; 
		}

		private string defaultSSML;
		private SpeechSynthesizer synthesizer;
		private Language defaultLanguage;
		private List<Language> availableLanguages;
		private ResourceManager resourceManager;
		private void GetDefaultSSML()
		{
			var assembly = typeof(ILanguageService).GetTypeInfo().Assembly;
			Stream stream = assembly.GetManifestResourceStream("BabySmash.Core.Resources.ssml_character_default.xml");
			using(var reader = new System.IO.StreamReader(stream)) {
				this.defaultSSML = reader.ReadToEnd();
			}
		}
	}
}
