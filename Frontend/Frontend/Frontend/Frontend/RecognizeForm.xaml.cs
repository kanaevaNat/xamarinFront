﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Frontend.dto;
using Frontend.holder;
using Plugin.Media;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Frontend
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecognizeForm : ContentPage
    {
        public RecognizeForm()
        {
            InitializeComponent();
        }


        private async void GetPhoto(object sender, EventArgs e)
        {
            try
            {
                await CrossMedia.Current.Initialize();
                // выбираем фото
                var photo = await CrossMedia.Current.PickPhotoAsync();
                ImageVal.Source = ImageSource.FromFile(photo.Path);
                byte[] imageArray = null;

                using (MemoryStream memory = new MemoryStream()) {

                    Stream stream = photo.GetStream();
                    stream.CopyTo(memory);
                    imageArray = memory.ToArray();
                }

                string base64String = System.Convert.ToBase64String(imageArray);
                RecognizeRequest request = new RecognizeRequest()
                {
                    Base64Image = base64String
                };
                Response response = await App.ApiClient.Recognize(request);
                if (response.Code != HttpStatusCode.OK)
                {
                    await DisplayAlert("Сообщение об ошибке", response.Code.ToString(), "OK");
                    return;
                }

                Result.Text = ((RecognitionResponse)response.Data).Result;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Сообщение об ошибке", ex.Message, "OK");
            }
        }

        private async void SigOut(object sender, EventArgs e)
        {
            UserTokenHolder.Token = null;
            MainPage mainPage = new MainPage();
            await Navigation.PushAsync(mainPage);
        }
        
        private async void GoToHistory(object sender, EventArgs e)
        {
            Response response = await App.ApiClient.GetHistory();
            List<HistoryDto> history = (List<HistoryDto>)response.Data;
            HistoryPage historyPage = new HistoryPage();
            historyPage.BindingContext = history;
            await Navigation.PushAsync(historyPage);
        }

        private async void GoToGenerationHistory(object sender, EventArgs e)
        {
            Response response = await App.ApiClient.GetMyGenerationHistory();
            List<GenerationDto> generations = (List<GenerationDto>)response.Data;
            GenerationHistoryPage page = new GenerationHistoryPage();
            page.BindingContext = generations;
            await Navigation.PushAsync(page);
        }
        
        private async void GoToGenerate(object sender, EventArgs e)
        {
            GenerateImageRequest request = new GenerateImageRequest()
            {
                Promt = Result.Text
            };
            Response response = await App.ApiClient.GenerateResponse(request);
            GeneratePageDto pageDto = new GeneratePageDto()
            {
                Response = (GenerateImageResponse)response.Data,
                Promt = Result.Text
            };
            GenerateResultPage resultPage = new GenerateResultPage();
            resultPage.BindingContext = pageDto;
            await Navigation.PushAsync(resultPage);
        }

        private async void GoToPublicList(object sender, EventArgs e)
        {
            Response response = await App.ApiClient.GetPublicList();
            List<GenerationDto> generations = (List<GenerationDto>)response.Data;
            PublicViewList list = new PublicViewList();
            list.BindingContext = generations;
            await Navigation.PushAsync(list);
        }
    }
}