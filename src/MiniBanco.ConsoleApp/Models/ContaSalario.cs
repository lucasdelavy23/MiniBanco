using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models;

public class ContaSalario : Conta
{
    private int saquesRealizadosNoMes;

    public string Empregador { get; }
    public int LimiteSaquesMensais { get; }
    public int SaquesRealizadosNoMes => this.saquesRealizadosNoMes;

    public ContaSalario(string titular, int numero, decimal saldoInicial,
        string empregador, int limiteSaquesMensais)
        : base(titular, numero, saldoInicial)
    {
        if (string.IsNullOrWhiteSpace(empregador))
        {
            throw new ArgumentException("O empregador é obrigatório.", nameof(empregador));
        }

        if (limiteSaquesMensais < 0)
        {
            throw new ArgumentException("O limite mensal de saques não pode ser negativo.", nameof(limiteSaquesMensais));
        }

        this.Empregador = empregador.Trim();
        this.LimiteSaquesMensais = limiteSaquesMensais;
        this.saquesRealizadosNoMes = 0;
    }

    public override void Sacar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("O valor do saque deve ser maior que zero.", nameof(valor));
        }

        bool cobrarTarifa = this.saquesRealizadosNoMes >= this.LimiteSaquesMensais;
        decimal tarifa = cobrarTarifa ? 5m : 0m;

        // Valida saque e tarifa juntos, antes de alterar qualquer estado.
        if (valor > this.Saldo || tarifa > this.Saldo - valor)
        {
            throw new SaldoInsuficienteException("Saldo insuficiente para o saque e sua eventual tarifa de R$ 5,00.");
        }

        decimal novoSaldo = this.Saldo - valor - tarifa;
        int novoContador = checked(this.saquesRealizadosNoMes + 1);

        RegistrarTransacao("Saque", "Saque", valor);
        if (cobrarTarifa)
        {
            RegistrarTransacao("Saque além do limite mensal", "Tarifa", tarifa);
        }

        this.Saldo = novoSaldo;
        this.saquesRealizadosNoMes = novoContador;
    }

    public override void AplicarFechamentoMensal()
    {
        this.saquesRealizadosNoMes = 0;
    }
}
