using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using ClipboardTranslator.Model;
using ClipboardTranslator.Utility;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;

using MessagePack;

using Prism.Commands;
using Prism.Mvvm;

namespace ClipboardTranslator.ViewModel
{
	public class MainViewModel : BindableBase
	{
		private readonly char[] Suffixes = { '.', '!', '?', ')', ':', '"', '*', '~' };
		private readonly int CacheCount = 1000000;

		public DelegateCommand LoadedCommand
		{
			get;
			private set;
		}

		public DelegateCommand ClosingCommand
		{
			get;
			private set;
		}

		public DelegateCommand ExitCommand
		{
			get;
			private set;
		}

		private string _CurrentClipboard;
		public string CurrentClipboard
		{
			get => _CurrentClipboard;
			set
			{
				_CurrentClipboard = value;
				RaisePropertyChanged();
			}
		}

		private string _OCRText;
		public string OCRText
		{
			get => _OCRText;
			set
			{
				_OCRText = value;
				RaisePropertyChanged();
			}
		}

		private string _TranslatedText;
		public string TranslatedText
		{
			get => _TranslatedText;
			set
			{
				_TranslatedText = value;
				RaisePropertyChanged();
			}
		}

		private bool _IsRunning;
		public bool IsRunning
		{
			get => _IsRunning;
			set
			{
				_IsRunning = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(WindowResizeMode));
				RaisePropertyChanged(nameof(WindowBorder));
				RaisePropertyChanged(nameof(WindowOpacity));

				if (!IsRunning)
				{
					CurrentClipboard = "";
					OCRText = "";
					TranslatedText = "";
				}
			}
		}

		public ResizeMode WindowResizeMode
		{
			get
			{
				return IsRunning ? ResizeMode.NoResize : ResizeMode.CanResizeWithGrip;
			}
		}

		public Thickness WindowBorder
		{
			get
			{
				return IsRunning ? new Thickness() : new Thickness(1, 1, 1, 1);
			}
		}

		public double WindowOpacity
		{
			get
			{
				return IsRunning ? 0 : 0.5;
			}
		}

		public bool IsInDesignMode
		{
			get
			{
				return DesignerProperties.GetIsInDesignMode(new DependencyObject());
			}
		}

		private GoogleCredential Credential;

		private readonly List<CacheObject> CacheList = new();
		private readonly Dictionary<string, string> CacheDictionary = new();

		public MainViewModel()
		{
			LoadedCommand = new DelegateCommand(OnLoaded);
			ClosingCommand = new DelegateCommand(OnClosing);
			ExitCommand = new DelegateCommand(() =>
			{
				Application.Current?.Shutdown();
			});
		}

		private void OnLoaded()
		{
			if (!IsInDesignMode)
			{
				LoadCache();

				Task.Run(() => CheckClipboardTask());
			}
		}

		private void OnClosing()
		{
			SaveCache();	
		}

		private void LoadCache()
		{
			if (!File.Exists("cache.bin"))
			{
				return;
			}

			var compressedBytes = File.ReadAllBytes("cache.bin");
			var bytes = Compression.Decode(compressedBytes);

			var cacheObjects = MessagePackSerializer.Deserialize<List<CacheObject>>(bytes);
			if (cacheObjects == null)
			{
				return;
			}

			for (int i = 0; i < cacheObjects.Count; i++)
			{
				var cache = cacheObjects[i];

				CacheList.Add(cache);
				CacheDictionary.Add(cache.Key, cache.Value);
			}
		}

		private void SaveCache()
		{
			var cacheObject = CacheList.Select((x, i) =>
			{
				x.Index = i;

				return x;
			}).ToList();

			var bytes = MessagePackSerializer.Serialize(cacheObject);
			var compressedBytes = Compression.Encode(bytes);

			File.WriteAllBytes("cache.bin", compressedBytes);
		}

		private async Task CheckClipboardTask()
		{
			while (true)
			{
				try
				{
					await Task.Delay(100);

					if (!IsRunning)
					{
						continue;
					}

					string clipboard = "";
					await App.Current?.Dispatcher?.InvokeAsync(() =>
					{
						try
						{
							clipboard = Clipboard.GetText();
						}
						catch (Exception e)
						{
							Trace.WriteLine(e.ToString());
						}
					});

					if (string.IsNullOrWhiteSpace(clipboard))
					{
						continue;
					}

					if (clipboard == "\r\n")
					{
						continue;
					}

					if (CurrentClipboard == clipboard)
					{
						continue;
					}

					CurrentClipboard = clipboard;

					string originalText = clipboard.Replace("\r\n", " ").Trim();
					OCRText = originalText;

					bool isEnded = false;
					for (int i = 0; i < Suffixes.Length; i++)
					{
						if (originalText[^1] == Suffixes[i])
						{
							isEnded = true;
							break;
						}
					}

					if (!isEnded)
					{
						continue;
					}

					string result;
					if (CacheDictionary.ContainsKey(originalText))
					{
						result = CacheDictionary[originalText];
					}
					else
					{
						result = await Translate(originalText);

						AddToCache(originalText, result);
					}

					TranslatedText = result;
				}
				catch (Exception e)
				{
					Trace.WriteLine(e.ToString());
				}
			}
		}

		private async Task<string> Translate(string input)
		{
			try
			{
				if (Credential == null)
				{
					Credential = GetCredential();
				}

				var client = TranslationClient.Create(Credential);
				var result = await client.TranslateTextAsync(input, "ko", "en");

				return result.TranslatedText;
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.ToString());
			}

			return "";
		}

		private static GoogleCredential GetCredential()
		{
			return GoogleCredential.FromFile("Translation-c73b6d7cfc59.json");
		}

		private void AddToCache(string originalText, string result)
		{
			if (CacheDictionary.ContainsKey(originalText))
			{
				return;
			}

			if (CacheList.Count >= CacheCount)
			{
				CacheDictionary.Remove(CacheList[0].Key);
				CacheList.RemoveAt(0);
			}

			CacheDictionary.Add(originalText, result);
			CacheList.Add(new CacheObject
			{
				Key = originalText,
				Value = result
			});
		}
	}
}
