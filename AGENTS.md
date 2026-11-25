# AGENTS.md — Unity Game Project Instructions

## 1. Objetivo

Este é um jogo acadêmico desenvolvido em Unity 6.

O projeto utiliza uma estrutura 2.5D:

- Ambiente/cenário em 3D.
- Backgrounds e elementos visuais podem utilizar imagens 2D.
- Personagem principal utiliza sprites 2D.
- NPCs também podem utilizar sprites 2D.
- O movimento principal é point-and-click.
- O projeto utiliza o New Input System da Unity.
- Textos de interface utilizam TextMeshPro.

O objetivo desta tarefa é ANALISAR o projeto existente e completar as mecânicas necessárias para formar um gameplay funcional.

NÃO reestruture o projeto inteiro.

NÃO substitua sistemas que já funcionam sem necessidade.

Antes de modificar qualquer arquivo, analise os scripts, cenas, prefabs e componentes existentes para entender a arquitetura atual.

---

# 2. REGRA PRINCIPAL

Antes de escrever código:

1. Analise a estrutura do projeto.
2. Localize os scripts existentes.
3. Identifique as cenas existentes.
4. Identifique os sistemas já implementados.
5. Identifique referências entre GameObjects e scripts.
6. Identifique sistemas persistentes com DontDestroyOnLoad.
7. Identifique o Canvas e os elementos de UI existentes.
8. Identifique o sistema atual de interação.
9. Identifique o sistema atual de diálogo.
10. Identifique o sistema atual de movimento.
11. Identifique o GameManager.
12. Identifique o sistema atual de timer.
13. Identifique qualquer implementação existente de moedas ou mineração.

Depois disso, implemente apenas o que estiver faltando.

Não crie sistemas duplicados.

Exemplos:

- Se já existe GameManager, estenda o GameManager.
- Se já existe MoneyManager, corrija/estenda o MoneyManager.
- Se já existe sistema genérico de interação, reutilize-o quando fizer sentido.
- Se já existe HUD, adapte-a.
- Se já existe TimerUI, preserve seu funcionamento.

---

# 3. Fluxo principal do jogo

O fluxo desejado atualmente é:

Main Menu
    ↓
Entrada na cena principal
    ↓
Jogador explora o ambiente
    ↓
Jogador encontra NPC
    ↓
Interage com NPC
    ↓
Diálogo
    ↓
Última fala do NPC
    ↓
Missão começa
    ↓
Timer de 20 minutos começa
    ↓
HUD de moedas aparece
    ↓
Jogador pode minerar
    ↓
Consegue moedas
    ↓
Objetivo: atingir 250 moedas
    ↓
250 moedas alcançadas
    ↓
Missão concluída / vitória

Se o tempo chegar a zero antes do objetivo:

Game Over.

---

# 4. Sistemas que já podem existir

Procure especialmente por scripts com nomes semelhantes a:

- GameManager.cs
- DialogueController.cs
- NPCInteraction.cs
- Interactable.cs
- PlayerClickMovement.cs
- TimerUI.cs
- MoneyManager.cs
- MoneyUI.cs
- MiningSpot.cs
- ScrollInteraction.cs

Os nomes podem ter sido alterados.

Não presuma que a implementação está correta apenas porque o arquivo existe.

Leia os scripts e compreenda como eles funcionam.

---

# 5. GameManager

Deve existir apenas uma fonte principal para o estado da missão.

O GameManager deve controlar ou expor:

- missão ativa;
- tempo restante;
- game over;
- vitória;
- início da missão;
- conclusão da missão.

A missão possui duração padrão de:

1200 segundos (20 minutos).

O início da missão acontece após o jogador concluir o diálogo inicial com o NPC.

Não quebre esse fluxo caso já esteja implementado.

Se GameManager utilizar:

DontDestroyOnLoad

preserve a persistência corretamente.

Evite referências persistentes inválidas para GameObjects pertencentes exclusivamente a uma cena.

---

# 6. Objetivo da missão

O objetivo inicial é:

250 moedas.

O jogador começa com:

0 moedas.

A missão deve ser concluída quando:

currentMoney >= 250

Não exigir exatamente 250.

Exemplo:

Jogador possui 240 moedas.

Encontra 25.

Resultado:

265 moedas.

A missão deve ser considerada concluída.

---

# 7. Sistema global de moedas

Implementar ou corrigir um sistema central de dinheiro.

Preferencialmente:

MoneyManager

Responsabilidades:

- armazenar dinheiro atual;
- adicionar dinheiro;
- consultar dinheiro;
- resetar dinheiro quando necessário;
- persistir corretamente quando houver troca de cena;
- informar outros sistemas quando o valor mudar.

Evitar fazer a UI consultar o valor desnecessariamente a cada frame se uma arquitetura baseada em eventos puder ser utilizada de forma simples.

Sugestão:

evento OnMoneyChanged.

Exemplo conceitual:

MoneyManager
    ↓
AddMoney(25)
    ↓
currentMoney += 25
    ↓
OnMoneyChanged
    ↓
MoneyUI atualiza

O sistema deve impedir adições inválidas quando apropriado.

---

# 8. HUD DE MOEDAS

Criar ou adaptar uma HUD que mostre:

MOEDAS: X / 250

Exemplos:

MOEDAS: 0 / 250
MOEDAS: 25 / 250
MOEDAS: 175 / 250
MOEDAS: 250 / 250

IMPORTANTE:

A HUD NÃO deve aparecer antes da missão começar.

Fluxo:

Antes do diálogo terminar:
HUD escondida.

Após StartMission():
HUD visível.

A HUD deve continuar funcionando corretamente após mudanças de cena, respeitando a arquitetura existente.

Não utilizar obrigatoriamente emoji Unicode para a moeda.

A fonte Anton SDF utilizada no projeto não possui suporte ao caractere Unicode U+1FA99.

Se houver necessidade de ícone de moeda, preferir:

Image + Sprite

em vez de inserir o emoji no TextMeshPro.

---

# 9. Sistema de mineração

O jogador deve poder entrar em uma área específica de mineração.

Quando estiver dentro da área:

MiningPanel deve aparecer.

Quando sair:

MiningPanel deve desaparecer.

O painel deve possuir ou suportar:

- título;
- botão MINERAR;
- barra de progresso;
- texto de resultado.

Fluxo:

Player entra na Mining Area
    ↓
MiningPanel aparece
    ↓
Player clica em MINERAR
    ↓
Mineração começa
    ↓
Barra progride durante 3 segundos
    ↓
Resultado é sorteado
    ↓
Mensagem aparece
    ↓
Moedas são adicionadas
    ↓
HUD global é atualizada

---

# 10. Condições para minerar

O jogador somente pode minerar quando:

- a missão estiver ativa;
- não estiver em Game Over;
- não tiver vencido;
- estiver dentro de uma área válida de mineração;
- nenhuma mineração já estiver em andamento.

Durante os 3 segundos de mineração:

- impedir múltiplos cliques;
- desabilitar temporariamente o botão MINERAR;
- preencher progressivamente a barra;
- mostrar feedback visual adequado.

Após concluir:

- habilitar novamente o botão, caso a mineração continue permitida;
- zerar/preparar a barra para uma nova tentativa;
- apresentar o resultado.

---

# 11. Tempo de mineração

Cada tentativa dura:

3 segundos.

A barra deve progredir visualmente de:

0 → 100%

durante esses 3 segundos.

Preferir Coroutine ou solução equivalente apropriada para Unity.

Não bloquear a thread principal.

NÃO utilizar:

Thread.Sleep()

ou qualquer operação bloqueante.

---

# 12. Recompensas da mineração

Uma mineração pode encontrar:

- 1 moeda
- 2 moedas
- 5 moedas
- 10 moedas
- 25 moedas
- 50 moedas
- 75 moedas
- 100 moedas

Os valores maiores devem ser progressivamente mais raros.

Utilizar inicialmente esta distribuição:

| Recompensa | Chance |
|-----------:|-------:|
| 1 moeda | 30% |
| 2 moedas | 22% |
| 5 moedas | 18% |
| 10 moedas | 12% |
| 25 moedas | 8% |
| 50 moedas | 5% |
| 75 moedas | 3% |
| 100 moedas | 2% |

Total:

100%.

IMPORTANTE:

As probabilidades devem ser configuráveis pelo Inspector sempre que possível.

Evite espalhar números mágicos pelo código.

Criar uma estrutura serializável para recompensas é desejável.

Exemplo conceitual:

MiningReward

- amount
- probability

Isso permitirá balanceamento futuro sem reescrever o código.

---

# 13. Possibilidade de não encontrar nada

O sistema deve suportar também o conceito de falha:

"Nenhuma moeda encontrada."

Entretanto, antes de implementar uma chance adicional de falha, verifique a configuração existente do projeto.

Caso não exista uma chance definida de falha, deixe-a configurável pelo Inspector, com valor inicial 0%, para não alterar silenciosamente a distribuição de 100% definida acima.

Exemplo:

Failure Chance = 0

Dessa forma podemos aumentar posteriormente sem alterar a arquitetura.

---

# 14. Resultado visual da mineração

Depois dos 3 segundos, mostrar uma mensagem.

Sucesso:

Você encontrou 25 moedas!

Falha:

Você não encontrou nenhuma moeda.

O texto deve ser atualizado no MiningPanel.

Evitar spam de Debug.Log como única forma de feedback.

Debug.Log pode existir para desenvolvimento, mas o jogador precisa receber feedback pela UI.

---

# 15. Animação / emote de mineração

Preparar a arquitetura para uma animação de mineração do personagem.

O Player utiliza sprite 2D.

Verifique se já existe:

- Animator;
- Animator Controller;
- Animation Clips;
- sprites de mineração;
- parâmetros de animação.

Se os assets necessários existirem, conectar uma animação de mineração ao fluxo.

Fluxo desejado:

Clique MINERAR
    ↓
Player inicia animação/emote de mineração
    ↓
barra de 3 segundos
    ↓
mineração termina
    ↓
Player retorna ao estado apropriado

Se os sprites/clips necessários NÃO existirem:

NÃO inventar referências quebradas.

Nesse caso:

- implementar a lógica;
- deixar o código preparado para disparar a animação posteriormente;
- documentar exatamente qual asset/configuração está faltando.

Não comprometer o funcionamento da mineração por falta de animação.

---

# 16. Movimento durante mineração

Idealmente, durante uma mineração ativa:

- impedir nova tentativa;
- impedir interação conflitante;
- evitar que o jogador saia andando enquanto está minerando.

Antes de alterar PlayerClickMovement, analise como o movimento atual funciona.

Implemente uma forma simples e reutilizável de bloquear/desbloquear movimento se isso puder ser feito sem quebrar o sistema existente.

Exemplo conceitual:

SetMovementEnabled(false)

e depois:

SetMovementEnabled(true)

Não reescreva o sistema de movimento inteiro apenas para isso.

---

# 17. Detecção da área de mineração

Verifique qual abordagem melhor se encaixa no projeto existente.

Pode utilizar:

- Trigger Collider;
- sistema Interactable existente;
- distância;
- outra solução já utilizada pelo projeto.

Se utilizar Trigger Collider:

- garantir Collider adequado;
- verificar necessidade de Rigidbody;
- evitar física desnecessária;
- respeitar a estrutura 2.5D existente.

O objetivo é:

Entrou na área → painel aparece.

Saiu da área → painel desaparece.

O botão MINERAR é responsável por iniciar a mineração.

Não exigir tecla E para iniciar a mineração, salvo se a arquitetura existente tornar isso necessário.

---

# 18. Vitória

Quando o jogador alcançar pelo menos:

250 moedas

o GameManager deve marcar a missão como concluída.

Exemplo:

if (money >= targetMoney)
{
    CompleteMission();
}

Após vitória:

- parar o timer;
- impedir novas minerações;
- impedir Game Over posterior;
- atualizar estado de vitória;
- apresentar feedback ao jogador.

Não é necessário criar uma tela final complexa caso ela não exista.

Pode ser criada uma integração simples/preparada para um VictoryPanel.

---

# 19. Game Over

Se o tempo chegar a:

00:00

antes de atingir 250 moedas:

Game Over.

Após Game Over:

- impedir mineração;
- manter timer em zero;
- impedir vitória posterior por ações indevidas;
- preparar ou mostrar feedback de Game Over caso já exista UI apropriada.

---

# 20. UI e referências de cena

Tenha cuidado com objetos persistentes.

GameManager e MoneyManager podem utilizar DontDestroyOnLoad.

Objetos como:

- Canvas
- MiningPanel
- MoneyHUD
- TextMeshProUGUI
- Buttons
- Sliders

podem pertencer somente à cena atual.

Não criar dependências frágeis onde um objeto persistente mantém uma referência destruída após SceneManager.LoadScene().

Escolha uma arquitetura segura.

---

# 21. Código

Manter código:

- simples;
- legível;
- organizado;
- compatível com Unity 6;
- compatível com New Input System;
- apropriado para um projeto acadêmico;
- fácil de explicar em apresentação.

Evitar overengineering.

Não adicionar frameworks externos desnecessários.

Não adicionar packages sem necessidade.

Não substituir TextMeshPro.

Não migrar o Input System.

Não alterar Render Pipeline.

---

# 22. Compatibilidade

Antes de finalizar:

- verificar erros de compilação;
- verificar namespaces;
- verificar referências nulas previsíveis;
- verificar métodos chamados por Buttons;
- verificar componentes necessários;
- verificar Tags/Layers quando utilizadas;
- verificar uso correto de Coroutine;
- verificar persistência entre cenas;
- verificar que a missão não inicia duas vezes;
- verificar que mineração não inicia duas vezes simultaneamente;
- verificar que vitória e Game Over são mutuamente exclusivos.

---

# 23. Testes mínimos esperados

Validar conceitualmente e, quando possível, executar/testar:

### Teste 1
Abrir a cena.

Esperado:
- timer escondido;
- HUD de moedas escondida;
- missão inativa.

### Teste 2
Concluir diálogo inicial.

Esperado:
- missão inicia;
- timer começa em 20:00;
- MoneyHUD aparece;
- mostra 0 / 250.

### Teste 3
Entrar na área de mineração.

Esperado:
- MiningPanel aparece.

### Teste 4
Sair da área.

Esperado:
- MiningPanel desaparece.

### Teste 5
Clicar MINERAR.

Esperado:
- botão fica temporariamente indisponível;
- barra progride por 3 segundos;
- apenas uma mineração ocorre.

### Teste 6
Finalizar mineração.

Esperado:
- recompensa é sorteada;
- resultado aparece;
- MoneyManager recebe o valor;
- MoneyHUD atualiza.

### Teste 7
Clicar repetidamente durante os 3 segundos.

Esperado:
- nenhuma mineração paralela é iniciada.

### Teste 8
Atingir mais de 250 moedas.

Exemplo:
240 + 25 = 265.

Esperado:
- vitória é acionada normalmente.

### Teste 9
Tempo chegar a zero antes de 250.

Esperado:
- Game Over;
- mineração fica indisponível.

### Teste 10
Trocar de cena, caso o jogo atualmente permita.

Esperado:
- estado global não é duplicado;
- GameManager não é duplicado;
- MoneyManager não é duplicado;
- referências de UI não geram MissingReferenceException.

---

# 24. IMPORTANTE: não alterar sem necessidade

Não faça mudanças cosméticas em arquivos que não estejam relacionados à tarefa.

Não renomeie scripts existentes sem necessidade.

Não mova arquivos sem necessidade.

Não altere cenas que não estejam relacionadas ao fluxo.

Não remova código funcional simplesmente para substituir pelo seu próprio estilo.

Faça alterações incrementais.

---

# 25. Antes de implementar

Faça primeiro uma análise do projeto e identifique:

1. Quais sistemas pedidos já existem.
2. Quais estão parcialmente implementados.
3. Quais estão faltando.
4. Quais arquivos precisam ser modificados.
5. Quais novos arquivos realmente precisam ser criados.
6. Quais referências precisarão ser configuradas no Unity Editor.
7. Quais assets necessários para animação estão disponíveis.

Depois implemente.

---

# 26. Depois de implementar

Ao terminar, forneça um resumo objetivo contendo:

## Arquivos criados
Liste os arquivos novos.

## Arquivos modificados
Liste os arquivos alterados e por quê.

## Mecânicas implementadas
Explique brevemente cada uma.

## Configuração manual necessária no Unity
Liste exatamente o que precisa ser arrastado/configurado no Inspector.

Exemplo:

- Player → Tag = Player
- MiningSpot → BoxCollider → Is Trigger = true
- MiningPanel → atribuir Button
- MiningPanel → atribuir Slider
- MoneyUI → atribuir MoneyText

## Assets faltantes
Informe qualquer sprite, animação, prefab ou referência que não pôde ser criada automaticamente.

## Testes
Informe o que foi validado e qualquer ponto que ainda precise ser testado manualmente.

---

# 27. Restrições finais

Prioridades, nesta ordem:

1. Não quebrar o que já funciona.
2. Manter o fluxo NPC → missão.
3. Manter o timer funcionando.
4. Implementar moedas globais.
5. Implementar HUD 0 / 250.
6. Implementar área de mineração.
7. Implementar mineração de 3 segundos.
8. Implementar recompensas probabilísticas.
9. Implementar condição de vitória.
10. Integrar animação de mineração se os assets permitirem.
11. Manter código simples o suficiente para ser explicado pelos alunos.

Analise o projeto real antes de tomar decisões arquiteturais.