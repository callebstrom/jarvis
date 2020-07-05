using System;
using System.Speech.Recognition;
using System.Collections.Immutable;
using System.Speech.Synthesis;

namespace jarvis {

    class App {

        private ImmutableList<ICommandConsumer> commandConsumers;
        private bool isRunning = false;
        private bool waitingForCommand = false;

        public App() {
            var commandConsumersBuilder = ImmutableList.CreateBuilder<ICommandConsumer>();
            commandConsumersBuilder.Add(new CSGOCommandConsumer());
            commandConsumers = commandConsumersBuilder.ToImmutable();
        }

        public void RunBlocking() {
            isRunning = true;

            using (
            var wakeUpRecogniser = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
            {
                var grammarBuilder = new GrammarBuilder("jarvis");
                var grammar = new Grammar(grammarBuilder);
                grammar.Name = "Jarvis Wake-Up";
                wakeUpRecogniser.LoadGrammar(grammar);

                wakeUpRecogniser.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(OnWakeUp);
                wakeUpRecogniser.SetInputToDefaultAudioDevice();
                wakeUpRecogniser.EndSilenceTimeout = TimeSpan.Zero;
                wakeUpRecogniser.RecognizeAsync(RecognizeMode.Multiple);
                while (isRunning)
                {
                    Console.ReadLine();
                }
            }
        }

        private void OnWakeUp(object sender, SpeechRecognizedEventArgs e) {

            using (
            var commandRecogniser = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
            {
                foreach (var consumer in commandConsumers)
                {
                    foreach (var grammarRaw in consumer.GetGrammar())
                    {
                        var grammarBuilder = new GrammarBuilder(grammarRaw);
                        var grammar = new Grammar(grammarBuilder);
                        grammar.Name = consumer.GetType().Name + "_" + grammarRaw;
                        commandRecogniser.LoadGrammar(grammar);
                    }
                }

                waitingForCommand = true;

                commandRecogniser.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(OnSpeechRecognised);
                commandRecogniser.SetInputToDefaultAudioDevice();
                commandRecogniser.EndSilenceTimeout = TimeSpan.FromMilliseconds(500);
                commandRecogniser.RecognizeAsync(RecognizeMode.Multiple);

                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                synthesizer.Volume = 75;
                synthesizer.Rate = 5;

                synthesizer.SpeakAsync("Yes?");

                while (waitingForCommand)
                {

                }
            }
        }

        private void OnSpeechRecognised(object sender, SpeechRecognizedEventArgs e) {
            var command = e.Result.Text.ToLower();

            Console.WriteLine("Running command: " + e.Result.Text);
            foreach (var consumer in commandConsumers)
            {
                consumer.Consume(command, "");
            }

            waitingForCommand = false;
        }
    }
}
