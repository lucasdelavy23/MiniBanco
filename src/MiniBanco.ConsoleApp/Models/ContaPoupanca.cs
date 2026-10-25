namespace MiniBanco.ConsoleApp.Models;

public class ContaPoupanca : Conta
{
    public double TaxaRendimentoMensal { get; }
    public int DiaAniversario { get; }

    public ContaPoupanca(string titular, int numero, decimal saldoInicial,
        double taxaRendimentoMensal, int diaAniversario)
        : base(titular, numero, saldoInicial)
    {
        if (!double.IsFinite(taxaRendimentoMensal) || taxaRendimentoMensal < 0
            || taxaRendimentoMensal >= (double)decimal.MaxValue)
        {
            throw new ArgumentException("A taxa deve ser não negativa, finita e representável em decimal.", nameof(taxaRendimentoMensal));
        }

        if (diaAniversario < 1 || diaAniversario > 31)
        {
            throw new ArgumentException("O dia de aniversário deve estar entre 1 e 31.", nameof(diaAniversario));
        }

        this.TaxaRendimentoMensal = taxaRendimentoMensal;
        this.DiaAniversario = diaAniversario;
    }

    public override void AplicarFechamentoMensal()
    {
        decimal taxa = (decimal)this.TaxaRendimentoMensal;
        decimal rendimento = checked(this.Saldo * (taxa / 100m));

        // No empate de meio centavo, arredonda para longe de zero.
        rendimento = decimal.Round(rendimento, 2, MidpointRounding.AwayFromZero);

        // O aniversário é informativo: cada chamada representa um novo ciclo mensal.
        if (rendimento > 0)
        {
            Depositar(rendimento, "Rendimento mensal da poupança");
        }
    }
}
