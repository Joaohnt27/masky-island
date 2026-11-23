# Etapa 04 — cristais e segunda lembrança

## Entrega

Após resolver a inscrição do céu, o jogador pode explorar a área de mineração existente na parte inferior direita de Island. O painel apresenta três veios selecionáveis: TRIÂNGULO, CÍRCULO e QUADRADO. São opções na mesma área existente, sem criar três novas áreas físicas.

Cada escavação dura três segundos e concede o cristal escolhido, uma única vez. Não há sorteio, falha aleatória, moedas ou prazo de missão.

Depois de coletar um cristal, selecioná-lo e clicar no encaixe correspondente:

| Encaixe | Pista | Cristal |
| --- | --- | --- |
| 1 | Sem cantos | Círculo |
| 2 | Três pontas | Triângulo |
| 3 | Quatro lados | Quadrado |

A ordem de preenchimento é livre. Erros mostram uma mensagem, sem consumir cristais ou apagar encaixes corretos. Os três encaixes completos revelam **“Na noite da tempestade, o farol se apagou.”** e registram a segunda lembrança. A aventura continua; o terceiro puzzle e o final ainda não foram implementados.

## Arquivos modificados

- Assets/Scripts/GameManager.cs: inventário de cristais e encaixes persistentes, validações, recompensa única da segunda lembrança e limpeza no reset. Continua sendo a fonte central de estado, sem referências a objetos de cena.
- Assets/Scripts/MiningSpot.cs: seleção de veio, pré-requisito da primeira lembrança, recompensa determinística, esgotamento dos veios e reset do estado local. Preserva coroutine, duração configurável, pausa, cancelamento, bloqueio de movimento e eventos opcionais de animação.
- Assets/Scripts/MiningUI.cs: seleção de cristais, encaixes, inventário, mensagens, indicação de área e botão FECHAR. Ao fechar durante uma escavação, ela é cancelada sem recompensa. Sair e retornar à área reabre o painel.
- Assets/Scripts/DialogueController.cs: NPC orienta para os cristais após a primeira lembrança e reconhece a segunda após resolver os encaixes.
- Assets/Scenes/Island.unity: reativados MiningSpot/MiningUI; adaptado o MinigPanel existente, preservando nomes e referências. Adicionados controles para três veios, três encaixes, inventário e fechamento. O painel usa o Canvas de menus já existente, atrás da pausa.
- Tests/GameplayValidation/FirstPuzzleValidation.cs: conversa de retorno agora percorre todas as falas do NPC, incluindo a orientação aos cristais.
- Tests/GameplayValidation/GameplayValidationRunner.cs: atualizadas expectativas antigas de mineração desativada e pré-requisito da primeira lembrança; evita execução simultânea com a suíte de cristais.

## Arquivos criados

- Tests/UIAuthoring/AuthorCrystalPuzzle.cs: autoria da UI em cópia isolada usando o Editor Unity.
- Tests/GameplayValidation/CrystalPuzzleValidation.cs: testes e renderização do painel.
- Tests/GameplayValidation/RunCrystalPuzzle.ps1: execução reproduzível em uma cópia independente em Builds/CV-<id>.
- Documentation/CrystalPuzzleValidationResults.txt e FirstPuzzleRegressionAfterCrystals.txt: relatórios das duas suítes.
- Documentation/CrystalPuzzlePreview.png e SecondMemoryPreview.png: painel antes/depois da solução.
- Este relatório.

Não foi necessário criar outro manager, inventário separado, sistema de interação ou script de runtime adicional.

## Configuração manual no Unity

Nenhuma necessária para jogar. Referências já salvas em Island:

- MiningSpot: Player atribuído, Interaction Distance = 2, Mining Duration = 3.
- MiningUI: painel, botão MINERAR, Slider, resultado, três Crystal Buttons (Triangle/Circle/Square), três Socket Buttons, CrystalInventory e MiningAreaHint atribuídos.
- Callbacks: SelectTriangle/SelectCircle/SelectSquare; PlaceFirst/PlaceSecond/PlaceThird; ClosePanel.
- O botão MINERAR continua recebendo seu listener pelo MiningUI.
- O painel permanece oculto até o jogador entrar na área; a primeira lembrança controla a disponibilidade dos comandos.

## Persistência e limites

Cristais coletados, encaixes preenchidos e lembranças sobrevivem a trocas entre Cave e Island. Reiniciar ou voltar ao menu limpa tudo. Não há salvamento em disco.

Pausa bloqueia seleção, encaixe e fechamento, congelando o progresso da escavação. Diálogo e outras interações que bloqueiam o movimento também impedem mineração e encaixes. Escavações incompletas são canceladas ao fechar, sair da área ou trocar de cena.

## Assets

Usados botões e nomes das formas, sem exigir emojis, novos sprites ou modelos. Os eventos de animação continuam preparados, mas faltam clips/controller de mineração. Arte própria dos cristais e animação do personagem podem ser adicionadas depois; a mecânica não depende delas.

## Validação

- **44 verificações aprovadas em Play Mode para cristais**: pré-requisitos, três segundos de progresso, recompensa determinística, bloqueio de tentativas paralelas, pausa, cancelamento por fechamento/saída/troca de cena, veios esgotados, encaixe incorreto sem perda, recompensa única, segunda lembrança, diálogo, persistência e limpeza no reinício/menu.
- **34 verificações de regressão do primeiro puzzle aprovadas**, incluindo conversa com NPC, solução, pausa, persistência e reinício. Total: **78 verificações aprovadas**.
- Compilação pela Unity concluída sem erros de código. Os avisos existentes de APIs de busca obsoletas não impedem execução.
- Inspeção visual do painel renderizado pela Unity em 1280 × 720, antes/depois da solução. Textos e controles conferidos; a área livre à esquerda permite voltar a clicar no cenário. A captura é uma renderização do Canvas, não um teste manual de navegação.
- IDs e referências internas de Cave/Island verificados. Cena e scripts de runtime modificados idênticos aos da cópia validada, conferidos por hash.
- A exceção conhecida do índice de busca do Editor em inicialização batch foi registrada separadamente como EDITOR NOTE. As suítes continuam falhando para exceções de gameplay. Logs brutos: Builds/CrystalWork/crystal-final.log e first-regression.log.
- Ainda recomendado experimentar manualmente a chegada à área por point-and-click e outras resoluções no Editor. Os testes automatizados posicionam o jogador e acionam os callbacks reais dos botões.

Para repetir a suíte a partir da raiz do projeto:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/GameplayValidation/RunCrystalPuzzle.ps1
```

## Como experimentar

1. Converse com o NPC e resolva SOL → LUA → ESTRELA no pergaminho central.
2. Vá à parte inferior direita da ilha até abrir o painel dos cristais.
3. Selecione cada veio e clique MINERAR; aguarde três segundos por cristal.
4. Selecione um cristal coletado e clique no encaixe correspondente à sua forma.
5. Complete os três encaixes, leia a lembrança e converse novamente com o NPC.

Próxima etapa: terceiro puzzle, terceira lembrança e integração do farol/final.
