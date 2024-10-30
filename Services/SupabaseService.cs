using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Supabase.Storage.Interfaces;
using Supabase.Storage;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;
using System.Net.Sockets;
using System.IO;
using FileOptions = Supabase.Storage.FileOptions;
using System.Security.AccessControl;
using Supabase.Gotrue;
using static Supabase.Gotrue.Constants;
using System.Management;
using System.IdentityModel.Tokens.Jwt;
using System.Data;


public class Paciente : BaseModel
{
    [PrimaryKey("id")]
    public Guid id { get; set; }
    public string Nome { get; set; }
    public int Idade { get; set; }
    public string MotivoDoenca { get; set; }
}

public class HistoricoChamada : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }
    public string UsuarioEmail { get; set; }
    public string NomeArquivo { get; set; }
    public string UrlImagem { get; set; }
    public string TextoPergunta { get; set; }
    public string RespostaIA { get; set; }
    public DateTime DataHora { get; set; }
}


// Classe de modelo para o cliente
public class Cliente : BaseModel
{
    [PrimaryKey("Id")]
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public int Idade { get; set; }
    public string Telefone { get; set; }
    public string Endereco { get; set; }
}


public class historicochamada : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }
    public string UsuarioEmail { get; set; }
    public string NomeArquivo { get; set; }
    public string UrlImagem { get; set; }
    public string TextoPergunta { get; set; }
    public string RespostaIA { get; set; }
    public DateTime DataHora { get; set; }
    public string UserRole { get; set; }
}

public class HistoricoChamada_duplicate : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }
    public string UsuarioEmail { get; set; }
    public string NomeArquivo { get; set; }
    public string UrlImagem { get; set; }
    public string TextoPergunta { get; set; }
    public string RespostaIA { get; set; }
    public DateTime DataHora { get; set; }
}

public class SupabaseService
{


    public async Task DeleteHistoricoChamada(Guid id)
    {
        await _supabase.From<HistoricoChamada>().Where(x => x.Id == id).Delete();
    }

    public readonly Supabase.Client _supabase;

    public async Task<string> ArmazenarSessaoComCodigoOAuth(string code)
    {
        // Troca o código OAuth por uma sessão completa
        var session = await _supabase.Auth.SignIn(code);

        if (session == null)
        {
            throw new InvalidOperationException("Erro ao obter a sessão usando o código OAuth.");
        }

        // Retorna a sessão resultante
        return session.ToString();
    }

    public async Task ArmazenarSessao(string accessToken, string refreshToken)
    {
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Os tokens de acesso e atualização não podem ser nulos ou vazios.");
        }

        await _supabase.Auth.SetSession(accessToken, refreshToken, false);

        var user = _supabase.Auth.CurrentUser;
        if (user == null)
        {
            throw new InvalidOperationException("Erro ao definir a sessão com o token de acesso.");
        }
    }



    //public async Task ArmazenarSessao(string accessToken)
    //{
    //    // Definindo manualmente o token de autenticação
    //    _supabase.Auth.SetAuth(accessToken);

    //    // Opcionalmente, você pode verificar se o usuário foi autenticado corretamente
    //    var user = _supabase.Auth.CurrentUser;
    //    if (user == null)
    //    {
    //        throw new InvalidOperationException("Erro ao definir a sessão com o token de acesso.");
    //    }
    //}


    public async Task<Session> FinalizarLoginComGoogle(string code)
    {
        // Usa o código para concluir o fluxo de autenticação
        var session = await _supabase.Auth.ExchangeCodeForSession(code, null);

        if (session == null)
        {
            throw new InvalidOperationException("Erro ao obter sessão com o código de OAuth.");
        }

        return session;
    }



    public async Task<UserInfo> ObterUsuarioAtual()
    {
        // Utilize o Supabase SDK para obter o usuário atual autenticado
        var user = _supabase.Auth.CurrentUser;

        if (user != null)
        {
            return new UserInfo
            {
                DisplayName = user.UserMetadata["full_name"]?.ToString() ?? "Usuário",
                Email = user.UserMetadata["email"]?.ToString() ?? "Email"
            };
        }
        return null;
    
    }

    public async Task<string> GetFacebookAuthUrl()
    {
        // Gera a URL para o provedor Google OAuth
        var authUrl = $"https://supabase.leandroti.uk/auth/v1/authorize?provider=facebook&redirect_to=https://supabase.leandroti.uk/auth/v1/callback";

        return authUrl;
    }

    public async Task<string> GetGoogleAuthUrl()
    {
        // Gera a URL para o provedor Google OAuth
        var authUrl = $"https://supabase.leandroti.uk/auth/v1/authorize?provider=google&redirect_to=https://supabase.leandroti.uk/auth/v1/callback";
         
        return authUrl;
    }

    public async Task<string> UploadImage(List<Paciente> _paciente, IBrowserFile file)
    {

        using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
        var buffer = new byte[file.Size];
        await stream.ReadAsync(buffer);


        // Nome do arquivo baseado no nome do arquivo original e um GUID para evitar conflitos
        var fileName = $"{_paciente[0].Nome}_{file.Name}";

        var imagePath = Path.Combine("Assets", fileName);

        // Cria um fluxo para o arquivo
        //using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024); // limite de 5MB

        // Faz o upload para o bucket 'paciente-imagens'
        await _supabase.Storage
        .From("paciente-imagens")
        .Upload(buffer, imagePath, null, null, true);




        //    // Retorna a URL pública da imagem
        return _supabase.Storage
        .From("paciente-imagens")
        .GetPublicUrl(fileName);
    }

    //public SupabaseService(string url, string anonKey)
    //{ 

    //    _supabase = new Supabase.Client(url, anonKey);
    //    Task.Run(async () => await _supabase.InitializeAsync()).Wait();

    //    if (_supabase == null)
    //    {
    //        throw new InvalidOperationException("Failed to initialize Supabase client.");
    //    }
    //}


    public SupabaseService(string url, string anonKey, string? accessToken = null, string? refreshToken = null)
    {
        _supabase = new Supabase.Client(url, anonKey);
        Task.Run(async () =>
        {
            await _supabase.InitializeAsync();

            // Define a sessão manualmente com o accessToken e refreshToken
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                await _supabase.Auth.SetSession(accessToken, refreshToken);
            }
        }).Wait();

        if (_supabase == null)
        {
            throw new InvalidOperationException("Failed to initialize Supabase client.");
        }
    }


    public SupabaseService(string url, string anonKey)
    {
        _supabase = new Supabase.Client(url, anonKey);
        _supabase.InitializeAsync().Wait();

        if (_supabase == null)
        {
            throw new InvalidOperationException("Failed to initialize Supabase client.");
        }

    }

    public async Task<List<Paciente>> GetPacientesEspecifico(Paciente paciente)
    {
        var response = await _supabase.From<Paciente>().Where(x => x.Nome == paciente.Nome && x.Idade == paciente.Idade).Get();
        return response.Models;
    }



    public async Task AddHistoricoChamada(historicochamada historico)
    {
        var _supabase = new Supabase.Client("https://supabase.leandroti.uk/", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyAgCiAgICAicm9sZSI6ICJhbm9uIiwKICAgICJpc3MiOiAic3VwYWJhc2UtZGVtbyIsCiAgICAiaWF0IjogMTY0MTc2OTIwMCwKICAgICJleHAiOiAxNzk5NTM1NjAwCn0.dc_X5iR_VP_qT0zsiyj_I_OZ2T9FtRU2BBNWN8Bu4GE");
        await _supabase.InitializeAsync();

        await _supabase.From<historicochamada>().Insert(historico);
      
    }


    public async Task ConfigureSession(string accessToken, string refreshToken)
    {
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Os tokens de acesso e atualização não podem ser nulos ou vazios.");
        }

        // Opcional: verificar a role no accessToken
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;
        var role = jsonToken?.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        Console.WriteLine("Role no token: " + role);  // Verifique se a role está correta

        await _supabase.Auth.SetSession(accessToken, refreshToken);
    }


    public async Task SetUserRole(string role)
    {
        await _supabase.Rpc("set_role", new { new_role = role });
    }

    public string GetUserIdFromToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

        // Captura o 'user_id' a partir dos claims do token
        var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        return userId;
    }



    public async Task<List<historicochamada>> GetHistoricoChamadas(string userEmail, string? accesstoken=null)
    {

        var _supabase = new Supabase.Client("https://supabase.leandroti.uk/", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyAgCiAgICAicm9sZSI6ICJhbm9uIiwKICAgICJpc3MiOiAic3VwYWJhc2UtZGVtbyIsCiAgICAiaWF0IjogMTY0MTc2OTIwMCwKICAgICJleHAiOiAxNzk5NTM1NjAwCn0.dc_X5iR_VP_qT0zsiyj_I_OZ2T9FtRU2BBNWN8Bu4GE");
        await _supabase.InitializeAsync();

        var response = await _supabase.From<historicochamada>()
       .Where(x => x.UsuarioEmail == userEmail )
       .Get();

        return response.Models;

    }



    public async Task AddCliente(Cliente cliente)
    { 
        var _supabase = new Supabase.Client("https://supabase.leandroti.uk/", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyAgCiAgICAicm9sZSI6ICJhbm9uIiwKICAgICJpc3MiOiAic3VwYWJhc2UtZGVtbyIsCiAgICAiaWF0IjogMTY0MTc2OTIwMCwKICAgICJleHAiOiAxNzk5NTM1NjAwCn0.dc_X5iR_VP_qT0zsiyj_I_OZ2T9FtRU2BBNWN8Bu4GE");
        await _supabase.InitializeAsync();


        await _supabase.From<Cliente>().Insert(cliente);
    }



     




    public async Task<List<Paciente>> GetPacientes()
    {
        var response = await _supabase.From<Paciente>().Get();
        return response.Models;
    }

    public async Task AddPaciente(Paciente paciente)
    {
        await _supabase.From<Paciente>().Insert(paciente);
    }

    public async Task UpdatePaciente(Paciente paciente)
    {
        await _supabase.From<Paciente>().Where(x => x.id == paciente.id).Update(paciente);
    }

    public async Task DeletePaciente(Guid id)
    {
        await _supabase.From<Paciente>().Where(x => x.id == id).Delete();
    }


    public async Task CadastrarCliente(string email, string password)
    {
        //var session = await _supabase.Auth.SignUp(email, password);

        // Primeiro, cadastre o usuário
        var session = await _supabase.Auth.SignUp(email, password);

        Console.WriteLine("sabino", session.ToString());
    }

    public async Task<bool> AutenticarClienteBool(string email, string password)
    {
        try
        {
            var session = await _supabase.Auth.SignIn(email, password);

            if (session.AccessToken == null)
            {
                return false;
            }
            else { return true; }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

    }

    public async Task AutenticarCliente(string email, string password)
    {
        var session = await _supabase.Auth.SignIn(email, password);

        Console.WriteLine("sabino", session.ToString());
    }


     public class UserInfo
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }

    public async Task ResetClient(string email)
    {
        //var session = await _supabase.Auth.SignUp(email, password);

        // Primeiro, cadastre o usuário
        var options = new SignInOptions { RedirectTo = "http://localhost:5057/login" };
        var didSendMagicLink = await _supabase.Auth.SendMagicLink(email, options);


        Console.WriteLine("sabino", didSendMagicLink.ToString());
    }


}
