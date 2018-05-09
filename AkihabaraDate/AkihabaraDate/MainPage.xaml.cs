using System;
using Xamarin.Forms;

namespace AkihabaraDate
{
	public partial class MainPage : ContentPage
	{
	    private readonly Model _model = new Model();

		public MainPage()
		{
			InitializeComponent();
            _model.Initialize();
		}

	    private void DatesStartButton_OnClicked(object sender, EventArgs e)
	    {
	        _model.RunDates();
	    }

	    private void DatesStopButton_OnClicked(object sender, EventArgs e)
	    {
	        _model.StopDates();
	    }

        private void VoiceStartButton_OnClicked(object sender, EventArgs e)
	    {
	        _model.ToggleTalk();
	    }

	    private void VoiceStopButton_OnClicked(object sender, EventArgs e)
	    {
            _model.StopVoice();
	    }
	}
}