namespace MiniBanco.ConsoleApp.Exceptions;

public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException(string mensagem) : base(mensagem)
    {
    }
}
