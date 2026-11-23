# Etapa 07 — cinco cenários, interação por clique e encerramento

## Entrega

A aventura usa cinco ambientes jogáveis: **Beach (praia), House (casa), Island (centro), Cave (caverna) e Lighthouse (farol)**. A cena adicional **Ending** apresenta a cutscene ilustrada e segue automaticamente para os créditos existentes.

Fluxo: menu → praia → primeira visita à casa → NPC no centro → puzzle do céu na casa → mineração e encaixes na caverna → releitura do bilhete na casa → QTE do farol → cutscene → créditos.

O NPC permanece no centro. As áreas não são bloqueadas por progresso; a missão atual orienta a exploração. A primeira conversa continua sendo responsável por iniciar a missão. Moedas e timer de missão não foram reintroduzidos.

## Mecânicas implementadas

- WASD movimenta o personagem; o clique em objetos, nomes das saídas ou caminhos inicia a aproximação e a interação. WASD cancela a aproximação automática. Os bloqueios existentes impedem movimento durante diálogos, leitura, mineração e QTE.
- Objetivo visível com localização e número de lembranças. O HUD e a dica de clique se escondem durante os painéis para não cobrir seu conteúdo.
- Inventário com **I**; **Esc** fecha o inventário ou pausa. Estado de cristais e lembranças permanece no GameManager ao mudar de área.
- A mesa da casa reutiliza o puzzle Sol → Lua → Estrela. Após a lembrança da tempestade, a mesma mesa abre o bilhete Barco → Peixe → Farol.
- Três veios visíveis na caverna usam o sistema de mineração existente: quatro segundos por tentativa, 35% de chance e sem cristais repetidos. Os cristais entram no inventário. Os encaixes são círculo, triângulo e quadrado; fechar o painel não impede reabri-lo clicando na área dos encaixes.
- O farol exige as três lembranças. O QTE solicita **A → D → W → Espaço**, com **2,5 segundos por comando**. Erro ou tempo esgotado permite tentar novamente sem perder progresso. A pausa congela o prazo. Esse prazo pertence apenas ao QTE, não é um timer global da missão.
- Acender a luz conclui a missão, mostra uma cutscene ilustrada de três tomadas com movimento, feixes de luz, legendas e transições, e abre os créditos. **Esc** permite pular a cutscene para os créditos. Jogar novamente reinicia na praia com o progresso limpo.

## Arquivos criados

- `Assets/Scenes/Beach.unity`, `House.unity`, `Lighthouse.unity`, `Ending.unity` e metadados.
- `Assets/Art/Backgrounds/Story/01-praia.png` a `05-farol.png` e metadados de importação.
- `Assets/Scripts/StoryLocation.cs`: chegada e objetivo local, com apresentação do HUD respeitando os painéis.
- `Assets/Scripts/EndingCutscene.cs`: sequência ilustrada e entrada nos créditos.
- `Assets/Scripts/BeaconBeamGraphic.cs`: feixes de luz da cutscene.
- `Tests/UIAuthoring/AuthorStoryWorld.cs`: montagem das cenas em cópia isolada. Não executar sobre o projeto principal sem preparar os templates originais.
- `Tests/GameplayValidation/StoryValidation.cs` e `RunStory.ps1`: validação de gameplay em cópia isolada.
- `Documentation/StoryWorld/`: resultados e capturas da integração.

## Arquivos modificados

- `Assets/Scenes/Island.unity`: centro com NPC e caminhos; `Cave.unity`: mineração e encaixes; `MainMenu.unity`: instruções de controles atualizadas.
- `ProjectSettings/EditorBuildSettings.asset`: cenas incluídas na sequência de build.
- `GameManager.cs`: reconhecimento das cinco áreas, visita à casa e reinício na praia.
- `MainMenuController.cs`: início na praia; `SceneFade.cs`: identificação da origem da transição.
- `PlayerClickMovement.cs`: WASD, limites locais, aproximação, cliques e dicas dos objetos.
- `Interactable.cs`, `NPCInteraction.cs`, `DoorInteraction.cs`: reutilização da interação e pontos de aproximação.
- `ScrollInteraction.cs`: duas leituras na mesma mesa; `DialogueController.cs`: orientações dos novos locais.
- `MiningSpot.cs`, `MiningUI.cs`: identificação dos encaixes, reabertura do painel e instruções da caverna.
- `LighthouseInteraction.cs`: QTE, falha, nova tentativa, pausa e encerramento.
- `MissionResultUI.cs`: o final novo usa a cutscene em vez do antigo painel de vitória.
- `SettingsMenuUI.cs`: instruções de WASD, clique e inventário.

Os scripts acima ficam em `Assets/Scripts/`. Sistemas de menu, configurações, pausa, inventário e progresso foram reaproveitados.

## Configuração manual no Unity

Nenhuma referência precisa ser arrastada. Cenas, fundos, botões, alvos de clique, pontos de aproximação, inventário e build settings ficam configurados.

Para jogar, abra **Assets/Scenes/MainMenu.unity** e dê **Play**. Se uma das cenas antigas estiver aberta no Editor, reabra a cena salva antes de testar as alterações.

Para balancear: **Cave → MiningSpot → Mining Duration / Crystal Chance**; **Lighthouse → LighthouseMechanism → Step Window**. Os cenários são imagens com objetos de interação em 3D e sprites de personagens, mantendo a estrutura 2.5D.

## Assets e limites

As cinco artes aprovadas estão integradas. A cutscene usa ilustrações animadas por código, sem vídeo externo nem dublagem. O emote de picareta existente foi mantido; não há novos clips desenhados de mineração ou caminhada. Não há salvamento em disco.

As áreas de movimento usam limites simples; as ilustrações ainda não estão separadas em camadas para oclusão individual de todos os objetos. A conferência visual automatizada usa 1280 × 720. Recomenda-se uma sessão manual para avaliar escala, ritmo da mineração com a chance real e outras proporções de tela.

## Validação

**Resultado: 38 verificações de gameplay e 2 verificações de apresentação aprovadas no Unity.** Compilação sem erros de código. A exceção conhecida do índice de busca do Editor foi registrada separadamente, sem ocultar erros do gameplay. Cenas e scripts entregues conferidos contra a cópia validada; referências de assets resolvidas.

O relatório detalhado da execução final e as capturas ficam em `Documentation/StoryWorld/`. O percurso inclui entrada pelo menu, movimento e pausa, cliques nos destinos/mesa/NPC, início único da missão, puzzles, extrações completas, inventário, encaixe incorreto, reabertura dos encaixes, retorno à casa, requisitos do farol, QTE com erro/timeout/pausa/acerto, cutscene, créditos e reinício.

Os testes controlam a chance da mineração em 0% e 100% somente durante a execução, para verificar falha e sucesso sem depender do sorteio. As cenas entregues mantêm 35%. As capturas também verificam o uso das novas artes e a ausência de sobreposição do HUD sobre os puzzles.
