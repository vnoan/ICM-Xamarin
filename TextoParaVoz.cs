using System;
using Android.Speech.Tts;
using System.Collections.Generic;
using Android.Content;

namespace CameraAppDemo
{
    class TextoParaVoz : Java.Lang.Object, TextToSpeech.IOnInitListener
    {
        TextToSpeech speaker; string toSpeak;
        Context c;
        public TextoParaVoz(Context con)
        {
            c = con;
        }

        public void Speak(string text)
        {
            toSpeak = text;
            if (speaker == null)
            {
                speaker = new TextToSpeech(c, this);
            }
            else
            {
                var p = new Dictionary<string, string>();
                speaker.Speak(toSpeak, QueueMode.Flush, p);
                System.Diagnostics.Debug.WriteLine("spoke " + toSpeak);
            }
        }

        #region IOnInitListener implementation
        public void OnInit(OperationResult status)
        {
            if (status.Equals(OperationResult.Success))
            {
                System.Diagnostics.Debug.WriteLine("speaker init");
                var p = new Dictionary<string, string>();
                speaker.Speak(toSpeak, QueueMode.Flush, p);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("was quiet");
            }
        }
        #endregion
    }
}