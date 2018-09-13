﻿using BabySmash.Core.Models;
using BabySmash.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using static BabySmash.Core.Utils;

namespace BabySmash.Core.ViewModels
{
	public class MainViewModel : BaseViewModel, IDisposable
	{
		private const string helloMessage = "helloMessage";
		private const string introSound = "BabySmash.Core.Resources.Sounds.EditedJackPlaysBabySmash.wav";
		private const int timerDelay = 30 * 1000;
		private IInteractionService interactionService;
		private ISpeakService speakService;
		private ISoundService soundService;
		private ILanguageService languageService;
		private IDeviceService deviceService;
		private Timer timer;
		private bool disposed;
		public MainViewModel(
			IInteractionService interactionService,
			ISoundService soundService,
			ISpeakService speakService,
			ILanguageService languageService,
			IDeviceService deviceService)
		{

			this.interactionService = interactionService;
			this.soundService = soundService;
			this.speakService = speakService;
			this.languageService = languageService;
			this.deviceService = deviceService;

			this.timer = new Timer((obj) => {
				CheckFiguresToRemove();
			}, null, timerDelay, timerDelay);

			this.interactionService.InteractionOccured += InteractionService_InteractionOccured;

			//play the intro sound
			this.soundService.PlayEmbebedResourceAsync(introSound);
		}
	
		public void Dispose()
		{
			if(this.disposed)
				return;
			this.disposed = true;

			if(this.timer != null) {
				this.timer.Dispose();
				this.timer = null;
			}

			if(this.interactionService != null) {
				this.interactionService.InteractionOccured -= InteractionService_InteractionOccured;
				this.interactionService = null;
			}

			if(this.speakService != null) {
				this.speakService = null;
			}
		}

		public string HelloMessage
		{
			get
			{
				return this.languageService.GetResourceText(helloMessage);
			}
		}

		private ObservableCollection<Figure> _figures = new ObservableCollection<Figure>();
		public ObservableCollection<Figure> Figures
		{
			get
			{
				return _figures;
			}
			set
			{
				SetField(ref _figures, value);
			}
		}
	
		private async void InteractionService_InteractionOccured(object sender, InteractionEventArgs e)
		{
			switch (e.Interaction) {
				case Models.InteractionType.MouseClick:
				break;
				case Models.InteractionType.MouseMove:
				break;
				case Models.InteractionType.KeyPress:
				await ProcessKey(e.Key);
				break;
				case Models.InteractionType.Exit:
				Clear();
				break;
				default:
				break;
			}
		}

		private void Clear()
		{
			Figures.Clear();
		}

		private async Task ProcessKey(string key)
		{
			if(string.IsNullOrEmpty(key))
				return;

			//could be a letter or number
			if(key.Length == 1) {
				// If a number is pressed, display the number.
				if(Regex.IsMatch(key, @"^[0-9]+$")) {
					int number;
					if(int.TryParse(key, out number))
						await AddNumber(int.Parse(key));
				}
				// If a letter was pressed, display the letter.
                else
                {
				    await AddLetter(key[0]);
                }
			} else {
				// Otherwise, display a random shape.
				await AddFigureAsync(new ShapeFigure());
			}

			CheckFiguresToRemove();
		}

		private void CheckFiguresToRemove()
		{
			if(Figures.Count >= Settings.Default.ClearAfter)
				Device.BeginInvokeOnMainThread(() => {
					Figures.RemoveAt(0);
				});

			for(int i = Figures.Count-1; i >= 0; i--) {
				var shape = Figures[i];
				if(!shape.IsVisible) {
					Device.BeginInvokeOnMainThread(() => {
						Figures.Remove(shape);
					});
				}
			}
		}

		private Task AddLetter(char letter)
		{
			var nl = new LetterFigure(letter);
			return AddFigureAsync(nl);
		}

		private Task AddNumber(int number)
		{
			var nf = new NumberFigure(number);
			return AddFigureAsync(nf);
		}

		private async Task AddFigureAsync(Figure figure)
		{			
			figure.StrokeColor = GetRandomColor();
			figure.FillColor = GetRandomColor();
			figure.FontSize = Settings.Default.FontSize;
			figure.Size = new Size(Settings.Default.FigureSize,  Settings.Default.FigureSize);

			var availableWidth = this.deviceService.GetScreenWidth();
			var availableHeight = this.deviceService.GetScreenHeight();
		
			var x = RandomBetweenTwoNumbers(0, Convert.ToInt32(availableWidth - figure.Size.Width));
			var y = RandomBetweenTwoNumbers(0, Convert.ToInt32(availableHeight - figure.Size.Height));
			figure.Position = new Point(x, y);

			Figures.Add(figure);
			await Speak(figure);
		}

		private async Task Speak(Figure figure)
		{
			if(!Settings.Default.Speak)
				return;

			var shape = figure as ShapeFigure;
			if(shape != null) {
				var textToRead = this.languageService.GetLanguageTextForShape(shape.Type);
				if(textToRead == null)
					textToRead = shape.ToString();
				await this.speakService.SpeakTextAsync(textToRead);
			} else {
				await this.speakService.SpeakSSMLAsync(this.languageService.GetLanguageTextForLetter(figure.ToString(), this.speakService.Language));
			}
		}
	}
}
