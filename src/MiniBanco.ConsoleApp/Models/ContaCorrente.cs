using MiniBanco.ConsoleApp.Exceptions;

namespace MiniBanco.ConsoleApp.Models;

public class ContaCorrente : Conta
{
    public decimal LimiteChequeEspecial { get; }

    public ContaCorrente(string titular, int numero, decimal saldoInicial, decimal limiteChequeEspecial)
        : base(titular, numero, saldoInicial)
    {
        if (limiteChequeEspecial < 0)
        {
            throw new ArgumentException("O limite do cheque especial não pode ser negativo.", nameof(limiteChequeEspecial));
        }

        this.LimiteChequeEspecial = limiteChequeEspecial;
    }

    public override void Sacar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("O valor do saque deve ser maior que zero.", nameof(valor));
        }

        // Com saldo negativo, valida antes da subtração para evitar estouro
        // em saques que já devem ser recusados por falta de limite.
        if (this.Saldo < 0 && valor > this.Saldo + this.LimiteChequeEspecial)
        {
            throw new SaldoInsuficienteException("Saldo e limite do cheque especial insuficientes para o saque.");
        }

        decimal novoSaldo = checked(this.Saldo - valor);

        // Comparar o saldo resultante evita somar saldo e limite muito altos.
        if (novoSaldo < -this.LimiteChequeEspecial)
        {
            throw new SaldoInsuficienteException("Saldo e limite do cheque especial insuficientes para o saque.");
        }

        RegistrarTransacao("Saque", "Saque", valor);
        this.Saldo = novoSaldo;
    }

    public override void AplicarFechamentoMensal()
    {
        // A manutenção é uma dívida obrigatória, mesmo quando ultrapassa o limite.
        decimal novoSaldo = checked(this.Saldo - 10m);
        RegistrarTransacao("Manutenção mensal da conta corrente", "Tarifa", 10m);
        this.Saldo = novoSaldo;
    }
}
