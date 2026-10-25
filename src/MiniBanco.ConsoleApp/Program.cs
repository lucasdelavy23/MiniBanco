using System.Globalization;
using System.Text;
using MiniBanco.ConsoleApp.Exceptions;
using MiniBanco.ConsoleApp.Models;
using MiniBanco.ConsoleApp.Services;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo cultura = CultureInfo.GetCultureInfo("pt-BR");
CultureInfo.CurrentCulture = cultura;
List<Conta> contas = new List<Conta>();
MotorAnalise motor = new MotorAnalise();
bool executando = true;

while (executando)
{
    Console.WriteLine("\n1 - Abrir Nova Conta");
    Console.WriteLine("2 - Registrar Movimentação");
    Console.WriteLine("3 - Aplicar Fechamento Mensal");
    Console.WriteLine("4 - Exibir Relatório das Contas");
    Console.WriteLine("0 - Sair");
    try
    {
        switch (LerInteiro("Opção: "))
        {
            case 1: AbrirConta(); break;
            case 2: RegistrarMovimentacao(); break;
            case 3: AplicarFechamento(); break;
            case 4: motor.ExibirRelatorios(contas); break;
            case 0: executando = false; break;
            default: Console.WriteLine("Opção inválida. Escolha uma opção do menu."); break;
        }
    }
    catch (SaldoInsuficienteException erro)
    {
        Console.WriteLine($"Operação recusada: {erro.Message}");
    }
    catch (ArgumentException erro)
    {
        Console.WriteLine($"Dados inválidos: {erro.Message}");
    }
    catch (FormatException)
    {
        Console.WriteLine("Formato inválido. Digite números e use vírgula para os centavos, por exemplo: 10,50.");
    }
    catch (OverflowException)
    {
        Console.WriteLine("Valor fora da capacidade numérica do programa. A operação não foi concluída.");
    }
    catch (EndOfStreamException)
    {
        executando = false;
    }
}
Console.WriteLine("Programa encerrado. Os dados em memória não serão salvos.");

void AbrirConta()
{
    Console.WriteLine("1 - Corrente | 2 - Poupança | 3 - Salário");
    int tipo = LerInteiro("Tipo da conta: ");
    if (tipo < 1 || tipo > 3)
        throw new ArgumentException("Escolha um tipo de conta entre 1 e 3.");
    string titular = LerTexto("Titular: ");
    int numero = LerInteiro("Número: ");
    if (BuscarConta(numero) != null)
    {
        Console.WriteLine("Já existe uma conta com esse número.");
        return;
    }
    decimal saldoInicial = LerDecimal("Saldo inicial: ");
    Conta novaConta;
    switch (tipo)
    {
        case 1:
            decimal limite = LerDecimal("Limite do cheque especial: ");
            novaConta = new ContaCorrente(titular, numero, saldoInicial, limite);
            break;
        case 2:
            double taxa = LerDouble("Taxa de rendimento mensal (%): ");
            int dia = LerInteiro("Dia de aniversário (1 a 31): ");
            novaConta = new ContaPoupanca(titular, numero, saldoInicial, taxa, dia);
            break;
        case 3:
            string empregador = LerTexto("Empregador: ");
            int limiteSaques = LerInteiro("Limite mensal de saques sem tarifa: ");
            novaConta = new ContaSalario(titular, numero, saldoInicial, empregador, limiteSaques);
            break;
        default:
            throw new ArgumentException("Tipo de conta inválido.");
    }
    contas.Add(novaConta);
    Console.WriteLine($"Conta {novaConta.Numero} aberta com sucesso.");
}

void RegistrarMovimentacao()
{
    int numero = LerInteiro("Número da conta: ");
    Conta? conta = BuscarConta(numero);
    if (conta == null)
    {
        Console.WriteLine("Conta não encontrada.");
        return;
    }
    Console.WriteLine("1 - Depósito | 2 - Saque");
    switch (LerInteiro("Movimentação: "))
    {
        case 1:
            string descricao = LerTexto("Descrição: ");
            decimal deposito = LerDecimal("Valor do depósito: ");
            conta.Depositar(deposito, descricao);
            break;
        case 2:
            decimal saque = LerDecimal("Valor do saque: ");
            conta.Sacar(saque);
            break;
        default:
            throw new ArgumentException("Escolha 1 para depósito ou 2 para saque.");
    }
    Console.WriteLine($"Movimentação realizada. Saldo atual: {conta.Saldo.ToString("C2", cultura)}");
}

void AplicarFechamento()
{
    if (contas.Count == 0)
    {
        Console.WriteLine("Nenhuma conta aberta até o momento.");
        return;
    }
    Console.WriteLine("Cada execução representa um novo ciclo mensal.");
    foreach (Conta conta in contas)
    {
        try
        {
            // O override da conta define a regra de fechamento por polimorfismo.
            conta.AplicarFechamentoMensal();
            Console.WriteLine($"Fechamento aplicado à conta {conta.Numero}.");
        }
        catch (OverflowException)
        {
            // Uma falha numérica não impede o fechamento das demais contas.
            Console.WriteLine($"Fechamento não aplicado à conta {conta.Numero}: valor fora da capacidade numérica.");
        }
    }
}

Conta? BuscarConta(int numero)
{
    foreach (Conta conta in contas)
    {
        if (conta.Numero == numero) return conta;
    }
    return null;
}

string LerTexto(string mensagem)
{
    Console.Write(mensagem);
    string? entrada = Console.ReadLine();
    if (entrada == null) throw new EndOfStreamException();
    return entrada.Trim();
}

int LerInteiro(string mensagem)
{
    return int.Parse(LerTexto(mensagem), NumberStyles.Integer, cultura);
}

decimal LerDecimal(string mensagem)
{
    // Sem separador de milhar para evitar interpretar 10.50 como 1050 reais.
    return decimal.Parse(LerTexto(mensagem), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, cultura);
}

double LerDouble(string mensagem)
{
    return double.Parse(LerTexto(mensagem), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, cultura);
}
