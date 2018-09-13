
using BabySmash.Core.Models;
using BabySmash.Core.Services;
using System;
using Windows.UI.Core;

namespace BabySmash.Windows.Services
{
	public class InteractionService : IInteractionService, IDisposable
	{
		private bool isCtrlKeyPressed;
		private bool disposed;
		private bool isKeyboardShowing;
		private bool isUsingSoftKeyboard;
		public InteractionService()
		{
            CoreWindow.GetForCurrentThread().CharacterReceived += CharacterReceived;
		}

        private void CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            args.Handled = true;
            OnInteractionOccured(args.KeyCode == 24
                ? new InteractionEventArgs(InteractionType.Exit)
                : new InteractionEventArgs(InteractionType.KeyPress) {Key = ((char) args.KeyCode).ToString().ToUpper()});
        }

		private void OnInteractionOccured(InteractionEventArgs eventArgs)
		{
			var handler = InteractionOccured;
			if (handler != null) {
				InteractionOccured(this, eventArgs);	
			}
		}

		public event EventHandler<InteractionEventArgs> InteractionOccured;

		public void HandleMouseDown()
		{
			throw new NotImplementedException();
		}

		public void HandleMouseUp()
		{
			throw new NotImplementedException();
		}

		public bool IsHardKeyboardPresent
		{
			get
			{
				var keyboardCaps = new global::Windows.Devices.Input.KeyboardCapabilities();
				return keyboardCaps.KeyboardPresent == 0;
			}
		}

		private void ShowsoftKeyboard(bool show)
		{
		    if (this.IsHardKeyboardPresent && show)
            {
				this.isUsingSoftKeyboard = true;
				//show softkeyboard
            }
		}

        void KeyboardShowing(global::Windows.UI.ViewManagement.InputPane sender, global::Windows.UI.ViewManagement.InputPaneVisibilityEventArgs args)
        {
        }

        void HekyboardHiding(global::Windows.UI.ViewManagement.InputPane sender, global::Windows.UI.ViewManagement.InputPaneVisibilityEventArgs args)
        {
        }

		
		public void Dispose()
		{
			if (!this.disposed) {
				this.disposed = true;
			    CoreWindow.GetForCurrentThread().CharacterReceived -= CharacterReceived;
                if (this.isUsingSoftKeyboard) {
					//get rid of soft keyboard events
				}
			}
		}
	}
}
