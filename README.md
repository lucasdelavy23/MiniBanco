# MiniBanco.ConsoleApp

Motor de Gestão de Contas Bancárias desenvolvido como projeto acadêmico em
**C# e .NET 10**, para praticar Programação Orientada a Objetos em uma aplicação
de console.

## Objetivo

Simular a gestão de contas correntes, poupanças e salários, permitindo registrar
movimentações, aplicar regras de fechamento mensal e analisar o histórico das
contas por meio de pontuação e relatórios.

## Funcionalidades

- Abertura dos três tipos de conta, com validação dos dados e bloqueio de números duplicados.
- Registro de depósitos e saques, com atualização do saldo e do extrato.
- Uso do limite do cheque especial na conta corrente.
- Aplicação de manutenção, rendimento e reinício do contador de saques no fechamento mensal.
- Relatórios com dados da conta, movimentações, pontuação, índice de saúde e classificação.
- Listagem de tarifas como itens críticos e de saques como itens de atenção.
- Recomendações para a gerência conforme a classificação e as movimentações.
- Tratamento de entradas inválidas, saldo insuficiente e estouro numérico.

## Tecnologias e organização

- C# com destino `net10.0`.
- SDK utilizado no desenvolvimento: **.NET 10.0.402**.
- Dados em memória com `List<T>` e operações com laços tradicionais, sem LINQ.
- Valores monetários em `decimal` e apresentação no padrão brasileiro.

```text
MiniBanco/
├── .gitignore
├── README.md
└── src/
    └── MiniBanco.ConsoleApp/
        ├── MiniBanco.ConsoleApp.csproj
        ├── Program.cs
        ├── Exceptions/
        │   └── SaldoInsuficienteException.cs
        ├── Models/
        │   ├── Transacao.cs
        │   ├── Conta.cs
        │   ├── ContaCorrente.cs
        │   ├── ContaPoupanca.cs
        │   └── ContaSalario.cs
        └── Services/
            └── MotorAnalise.cs
```

`Program.cs` controla a interação com o usuário. As classes de `Models` concentram
as regras das contas e das transações. `MotorAnalise` calcula a pontuação e emite
os relatórios. A exceção personalizada informa quando um saque não é permitido
por falta de saldo ou limite.

A classe abstrata `Conta` reúne o comportamento comum. As subclasses usam
herança e sobrescrevem métodos com `override`. No fechamento, o programa chama
o mesmo método em cada conta, aplicando a regra correspondente por polimorfismo.

## Como executar

Instale o **SDK do .NET 10**. Para conferir a instalação:

```bash
dotnet --version
```

Clone o repositório e acesse a pasta:

```bash
git clone https://github.com/lucasdelavy23/MiniBanco.git
cd MiniBanco
```

Na raiz do projeto, compile e execute:

```bash
dotnet build src/MiniBanco.ConsoleApp
dotnet run --project src/MiniBanco.ConsoleApp
```

O programa apresenta o menu:

```text
1 - Abrir Nova Conta
2 - Registrar Movimentação
3 - Aplicar Fechamento Mensal
4 - Exibir Relatório das Contas
0 - Sair
```

Digite a opção e pressione Enter. Para valores decimais, use vírgula, como
`100,50`, sem separador de milhar. Na poupança, informe `0,5` para uma taxa de
0,5% ao mês. A opção `0` encerra o programa.

## Regras de negócio

### Contas e movimentações

- Titular obrigatório, número de conta positivo e único, saldo inicial não negativo.
- Saldo inicial positivo gera um depósito no extrato e participa da pontuação.
  Saldo inicial zero não gera transação.
- Transações aceitam apenas os tipos `Deposito`, `Saque` e `Tarifa`, com valor
  maior que zero e descrição preenchida. Seus dados não podem ser alterados após a criação.
- Depósitos e saques atualizam saldo e extrato. Operações recusadas por dados
  inválidos, saldo insuficiente ou estouro numérico preservam o estado da conta.

### Regras por tipo

| Conta    | Saques                                                                | Fechamento mensal                                      |
| -------- | --------------------------------------------------------------------- | ------------------------------------------------------ |
| Corrente | Permite usar o saldo e o limite do cheque especial.                   | Desconta R$ 10,00 de manutenção e registra uma tarifa. |
| Poupança | Permite sacar até o saldo disponível.                                 | Credita o rendimento calculado pela taxa mensal.       |
| Salário  | Permite sacar até o saldo disponível, considerando a eventual tarifa. | Zera o contador mensal de saques, sem manutenção.      |

O limite do cheque especial deve ser não negativo. A manutenção da conta
corrente é cobrada mesmo que deixe o saldo abaixo do limite permitido para
saques, representando uma dívida da conta.

Na poupança, a taxa é não negativa e expressa em porcentagem. O rendimento é
`saldo × taxa / 100`, arredondado para centavos com `MidpointRounding.AwayFromZero`:
no empate de meio centavo, arredonda para longe de zero. Se o resultado
arredondado for zero, não há registro. Por exemplo, R$ 1.000,00 a 0,5% gera
R$ 5,00 de rendimento, registrado como depósito.

Na conta salário, o empregador é obrigatório e o limite mensal de saques sem
tarifa deve ser não negativo. Cada saque além desse limite cobra R$ 5,00.
O saldo precisa cobrir o saque e a tarifa juntos. Saques recusados não aumentam
o contador. Com limite zero, a tarifa é cobrada desde o primeiro saque.

### Pontuação e classificação

| Transação                                      | Pontos |
| ---------------------------------------------- | ------ |
| Depósito, incluindo saldo inicial e rendimento | 10     |
| Saque                                          | 5      |
| Tarifa                                         | 0      |

```text
Pontuação máxima = quantidade de transações × 10
Índice de saúde = (pontuação obtida / pontuação máxima) × 100
```

As tarifas também entram na quantidade de transações usada no cálculo da
pontuação máxima. A classificação considera o índice antes do arredondamento
para exibição.

| Índice de saúde                      | Classificação    | Ação recomendada                                                     |
| ------------------------------------ | ---------------- | -------------------------------------------------------------------- |
| Maior ou igual a 90%                 | Conta Saudável   | Oferecer investimentos e avaliar aumento de limite.                  |
| Maior ou igual a 60% e menor que 90% | Conta em Atenção | Contato da gerência para orientação financeira e revisão de tarifas. |
| Menor que 60%                        | Conta em Risco   | Bloquear aumento de limite e abrir análise de crédito.               |

Contas sem transações recebem índice de 0% e classificação em risco. O relatório
informa a ausência de histórico e recomenda coletar movimentações antes de
concluir a avaliação financeira.

As ações gerenciais são recomendações exibidas no relatório; o programa não
executa bloqueios ou alterações de limite automaticamente.

## Limitações

- Os dados ficam somente em memória e são perdidos ao encerrar a aplicação.
- Não há banco de dados, autenticação, API ou interface web.
- Não há controle de competência mensal: cada execução do fechamento representa
  um novo ciclo. Repeti-lo cobra nova manutenção, credita novo rendimento e zera
  novamente o contador da conta salário.
- O dia de aniversário da poupança é armazenado entre 1 e 31, mas não restringe
  a data de execução do fechamento.
- O índice de saúde é uma regra acadêmica baseada nos tipos de transação, não
  uma avaliação real de crédito ou da situação financeira do titular.
- O histórico não armazena o saldo de cada instante, portanto não permite
  comprovar qual saque específico utilizou o cheque especial.
