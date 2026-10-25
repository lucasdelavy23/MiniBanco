using System.Globalization;
using MiniBanco.ConsoleApp.Models;

namespace MiniBanco.ConsoleApp.Services;

public class MotorAnalise
{
    private readonly CultureInfo cultura = CultureInfo.GetCultureInfo("pt-BR");

    public int CalcularPontos(Transacao transacao)
    {
        switch (transacao.Tipo)
        {
            case "Deposito": return 10;
            case "Saque": return 5;
            case "Tarifa": return 0;
            default: throw new ArgumentException("Tipo de transação inválido.", nameof(transacao));
        }
    }

    public long CalcularPontuacao(Conta conta)
    {
        long pontos = 0;
        foreach (Transacao transacao in conta.Extrato)
        {
            pontos += CalcularPontos(transacao);
        }
        return pontos;
    }

    public double CalcularIndiceSaude(Conta conta)
    {
        long pontuacaoMaxima = conta.Extrato.Count * 10L;
        if (pontuacaoMaxima == 0)
        {
            return 0;
        }
        return ((double)CalcularPontuacao(conta) / pontuacaoMaxima) * 100;
    }

    public string Classificar(double percentual)
    {
        if (!double.IsFinite(percentual) || percentual < 0 || percentual > 100)
        {
            throw new ArgumentException("O índice deve estar entre 0 e 100.", nameof(percentual));
        }

        // A classificação usa o índice original, antes da formatação para exibição.
        if (percentual >= 90) return "Conta Saudável";
        if (percentual >= 60) return "Conta em Atenção";
        return "Conta em Risco";
    }

    public void ExibirRelatorios(List<Conta> contas)
    {
        if (contas.Count == 0)
        {
            Console.WriteLine("Nenhuma conta aberta até o momento.");
            return;
        }

        foreach (Conta conta in contas)
        {
            ExibirRelatorio(conta);
        }
    }

    public void ExibirRelatorio(Conta conta)
    {
        List<Transacao> extrato = conta.Extrato;
        double percentual = CalcularIndiceSaude(conta);

        Console.WriteLine("\n----------------------------------------");
        Console.WriteLine($"Titular: {conta.Titular} | Número: {conta.Numero}");
        Console.WriteLine($"Saldo atual: {conta.Saldo.ToString("C2", this.cultura)}");

        switch (conta)
        {
            case ContaCorrente corrente:
                Console.WriteLine("Tipo: Conta Corrente");
                Console.WriteLine($"Limite do cheque especial: {corrente.LimiteChequeEspecial.ToString("C2", this.cultura)}");
                break;
            case ContaPoupanca poupanca:
                Console.WriteLine("Tipo: Conta Poupança");
                Console.WriteLine($"Taxa mensal: {poupanca.TaxaRendimentoMensal.ToString(this.cultura)}%");
                Console.WriteLine($"Dia de aniversário: {poupanca.DiaAniversario}");
                break;
            case ContaSalario salario:
                Console.WriteLine("Tipo: Conta Salário");
                Console.WriteLine($"Empregador: {salario.Empregador}");
                Console.WriteLine($"Limite mensal de saques sem tarifa: {salario.LimiteSaquesMensais}");
                Console.WriteLine($"Saques realizados no mês: {salario.SaquesRealizadosNoMes}");
                break;
        }

        Console.WriteLine("Movimentações:");
        if (extrato.Count == 0)
        {
            Console.WriteLine("Sem transações: não há histórico para análise; índice definido como 0%.");
        }
        foreach (Transacao transacao in extrato)
        {
            Console.WriteLine($"- {Formatar(transacao)} | Pontos: {CalcularPontos(transacao)}");
        }

        Console.WriteLine($"Pontuação obtida: {CalcularPontuacao(conta)} | Máxima: {extrato.Count * 10L}");
        Console.WriteLine($"Índice de saúde: {percentual.ToString("F2", this.cultura)}%");
        Console.WriteLine($"Classificação: {Classificar(percentual)}");

        bool temTarifas = ExibirItens(extrato, "Tarifa", "Itens críticos (tarifas):");
        bool temSaques = ExibirItens(extrato, "Saque", "Itens de atenção (saques):");

        Console.WriteLine("Recomendações para a gerência:");
        if (percentual >= 90)
        {
            Console.WriteLine("- Oferecer investimentos e avaliar aumento de limite.");
        }
        else if (percentual >= 60)
        {
            Console.WriteLine("- Contatar o titular para orientação financeira e revisão de tarifas.");
        }
        else
        {
            Console.WriteLine("- Bloquear aumento de limite e abrir análise de crédito.");
        }

        if (temTarifas) Console.WriteLine("- Revisar as tarifas e orientar sobre formas de reduzi-las.");
        if (temSaques) Console.WriteLine("- Acompanhar os saques e planejar as retiradas.");
        if (conta is ContaCorrente)
        {
            Console.WriteLine("- Orientar sobre custos e uso responsável do cheque especial.");
            if (conta.Saldo < 0)
            {
                Console.WriteLine("- Regularizar o saldo negativo; o extrato não comprova qual saque usou cheque especial.");
            }
        }
        if (extrato.Count == 0) Console.WriteLine("- Coletar histórico antes de concluir a avaliação financeira.");
    }

    private bool ExibirItens(List<Transacao> extrato, string tipo, string titulo)
    {
        Console.WriteLine(titulo);
        bool encontrou = false;
        foreach (Transacao transacao in extrato)
        {
            if (transacao.Tipo == tipo)
            {
                Console.WriteLine($"- {Formatar(transacao)}");
                encontrou = true;
            }
        }
        if (!encontrou) Console.WriteLine("Nenhum.");
        return encontrou;
    }

    private string Formatar(Transacao transacao)
    {
        return $"{transacao.Descricao} | {transacao.Tipo} | {transacao.Valor.ToString("C2", this.cultura)}";
    }
}
