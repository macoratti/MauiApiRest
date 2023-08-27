using CommunityToolkit.Mvvm.ComponentModel;
using MauiApiRest.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace MauiApiRest.ViewModels;

public partial class MainViewModel : ObservableObject
{
    HttpClient client;
    JsonSerializerOptions _serializerOptions;
    string baseUrl = "https://catalogo.macoratti.net/api/1";

    [ObservableProperty]
    public string _categoriaInfoId;
    [ObservableProperty]
    public string _categoriaInfoNome;
    [ObservableProperty]
    public Categoria _categoria;
    [ObservableProperty]
    public ObservableCollection<Categoria> _categorias;
    [ObservableProperty]
    private string _nome;
    [ObservableProperty]
    private string _imagemUrl;

    public MainViewModel()
    {
        client = new HttpClient();
        Categorias = new ObservableCollection<Categoria>();
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    //iniciar o consumo da api rest
    //retornar a coleção de categorias
    public ICommand GetCategoriasCommand =>
       new Command(async () => await CarregaCategoriasAsync());
    private async Task CarregaCategoriasAsync()
    {
        var url = $"{baseUrl}/categorias";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                var data = await JsonSerializer.DeserializeAsync<ObservableCollection<Categoria>>(responseStream, _serializerOptions);
                Categorias = data;
            }
        }
    }

    public ICommand GetCategoriaCommand =>
     new Command(async () =>
     {
         if (CategoriaInfoId is not null)
         {
             var categoriaId = Convert.ToInt32(CategoriaInfoId);
             if (categoriaId > 0)
             {
                 var url = $"{baseUrl}/categorias/{categoriaId}";
                 var response = await client.GetAsync(url);

                 if (response.IsSuccessStatusCode)
                 {
                     using (var responseStream =
                             await response.Content.ReadAsStreamAsync())
                     {
                         var data = await JsonSerializer
                          .DeserializeAsync<Categoria>(responseStream, _serializerOptions);
                         Categoria = data;
                     }
                 }
             }
         }
     });

        public ICommand AddCategoriaCommand =>
        new Command(async () =>
        {
            var url = $"{baseUrl}/categorias";

            if (CategoriaInfoNome is not null)
            {
                var categoria =
                  new Categoria
                  {
                      Nome = CategoriaInfoNome,
                      ImagemUrl = "https://www.macoratti.net/Imagens/lanches/pudim1.jpg"
                  };
                string json = JsonSerializer.Serialize<Categoria>(categoria, _serializerOptions);

                StringContent content =
                  new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                await CarregaCategoriasAsync();
            }
        });

    public ICommand UpdateCategoriaCommand =>
       new Command(async () =>
       {
           if (CategoriaInfoId is not null && CategoriaInfoNome is not null)
           {
               var categoriaId = Convert.ToInt32(CategoriaInfoId);
               var categoria = Categorias.FirstOrDefault(x => x.CategoriaId == categoriaId);

               var url = $"{baseUrl}/categorias/{categoriaId}";
               categoria.Nome = CategoriaInfoNome;

               string jsonResponse = JsonSerializer.Serialize<Categoria>(categoria, _serializerOptions);

               StringContent content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");

               var response = await client.PutAsync(url, content);
               await CarregaCategoriasAsync(); // Atualiza a lista de produtos
           }
       });

    public ICommand DeleteCategoriaCommand =>
         new Command(async () =>
         {
             if (CategoriaInfoId is not null)
             {
                 var categoriaId = Convert.ToInt32(CategoriaInfoId);
                 if (categoriaId > 0)
                 {
                     var url = $"{baseUrl}/categorias/{categoriaId}";
                     var response = await client.DeleteAsync(url);
                     await CarregaCategoriasAsync();
                 }
             }
         });
}
