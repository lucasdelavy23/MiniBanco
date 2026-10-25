using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models;

public abstract class Conta
{
    private readonly List<Transacao> extrato = new List<Transacao>();

    public string Titular { get; }
    public int Numero { get; }
    public decimal Saldo { get; protected set; }

    // Uma cópia permite consultar o extrato sem alterar o histórico da conta.
    public List<Transacao> Extrato => new List<Transacao>(this.extrato);

    public Conta(string titular, int numero, decimal saldoInicial)
    {
        if (string.IsNullOrWhiteSpace(titular))
        {
            throw new ArgumentException("O titular é obrigatório.", nameof(titular));
        }

        if (numero <= 0)
        {
            throw new ArgumentException("O número da conta deve ser positivo.", nameof(numero));
        }

        if (saldoInicial < 0)
        {
            throw new ArgumentException("O saldo inicial não pode ser negativo.", nameof(saldoInicial));
        }

        this.Titular = titular.Trim();
        this.Numero = numero;
        this.Saldo = saldoInicial;

        if (saldoInicial > 0)
        {
            RegistrarTransacao("Saldo inicial", "Deposito", saldoInicial);
        }
    }

    // Registra o histórico; a atualização do saldo cabe à operação bancária.
    public void RegistrarTransacao(string descricao, string tipo, decimal valor)
    {
        Transacao transacao = new Transacao(descricao, tipo, valor);
        this.extrato.Add(transacao);
    }

    public void Depositar(decimal valor, string descricao)
    {
        decimal novoSaldo = checked(this.Saldo + valor);
        RegistrarTransacao(descricao, "Deposito", valor);
        this.Saldo = novoSaldo;
    }

    public virtual void Sacar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("O valor do saque deve ser maior que zero.", nameof(valor));
        }

        if (valor > this.Saldo)
        {
            throw new SaldoInsuficienteException("Saldo insuficiente para realizar o saque.");
        }

        decimal novoSaldo = checked(this.Saldo - valor);
        RegistrarTransacao("Saque", "Saque", valor);
        this.Saldo = novoSaldo;
    }

    public virtual void AplicarFechamentoMensal()
    {
        // A conta base não aplica encargos; cada tipo define seu fechamento.
    }
}
