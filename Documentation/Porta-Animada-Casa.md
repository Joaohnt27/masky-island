# Porta animada da casa

Primeira aplicação do efeito de portas interativas, na praia e no centro da ilha. O resto das passagens mantém o comportamento existente.

## Experiência

- Cursor sobre a porta ou sobre a legenda Casa: abertura em meio segundo, com cinco quadros e interior iluminado.
- Cursor sai sem clique: fechamento reverso em 0,4 segundo.
- Clique: usa a aproximação já existente do personagem. A porta fica aberta mesmo se o cursor sair.
- Chegada ao ponto de acesso: a troca para House espera a abertura terminar e usa o SceneFade existente.
- WASD cancela a aproximação; se o cursor estiver fora, a porta fecha.
- Pausa congela o efeito e o som de mundo. Painéis que bloqueiam movimento também impedem o hover.

## Arte

A folha inclui a moldura, o batente e o vão opaco em todos os quadros. Isso permite cobrir a porta pintada no cenário sem redesenhar os backgrounds originais, que foram preservados. O brilho fica dentro da entrada. A moldura é posicionada sobre cada porta usando as coordenadas da pintura e o tamanho de cada cenário.

PNG: `Assets/Art/Doors/HouseDoor-v1.png`, criado pela ferramenta integrada de imagens com transparência e as pinturas da praia e do centro como referências.

## Arquivos

Criados: `Assets/Scripts/AnimatedDoor.cs`, a folha PNG e metadados, `Assets/Resources/CartoonSfx/DoorOpen.wav` e `DoorClose.wav` com metadados, `Tests/UIAuthoring/AuthorHouseDoor.cs` e `Tests/GameplayValidation/HouseDoorValidation.cs`.

Modificados: `DoorInteraction.cs` recebe uma referência opcional à animação; `PlayerClickMovement.cs` expõe o hover e o destino existentes e aguarda a abertura antes de entrar; `GameAudio.cs` inclui os dois efeitos de porta. As cenas Beach e Island recebem a peça visual e o componente AnimatedDoor. As áreas de clique principais ficam alinhadas às portas, preservando as legendas clicáveis e os pontos de aproximação existentes.

O novo componente reutiliza o raycast do movimento, sem consultar o mouse novamente. A animação usa progresso reversível, sem criar corrotinas concorrentes. Os sons são originais, sintetizados pelo utilitário de autoria, e respeitam as preferências de efeitos sonoros do jogo.

## Configuração manual

Nenhuma. Assets, referências e posições são configurados pelo utilitário na cópia de autoria e integrados após validação. Nenhum asset externo ou pacote adicional é necessário.

## Validação concluída

20 verificações aprovadas no Unity 6000.6.0f1, incluindo hover, quadros intermediários, reversão rápida, áudio, pausa, bloqueio por diálogo/inventário, clique com cursor fora, cancelamento por WASD, espera da abertura na chegada, transição para House e retorno às cenas. As animações do jogador e do NPC continuam presentes. Compilação sem erros; a exceção preexistente do índice de busca do Editor é filtrada somente quando originada em UnityEditor.Search.SearchDatabase.

Capturas da porta aberta e fechada na praia e no centro foram revisadas visualmente. Resultados e imagens estão em `Documentation/HouseDoor`. Os treze arquivos integrados foram comparados por hash com a cópia validada. As cenas anteriores foram guardadas em `Builds/HouseDoorBackup`.

## Prompt da arte

Create a game-ready sprite sheet of the SMALL STONE COTTAGE FRONT DOOR from these coastal storybook backgrounds, preserving the same hand-painted muted palette, thin ink contours, weathered warm brown vertical wooden planks, simple iron horizontal straps, rounded arched top and mossy gray-beige stone arch. Five frames in EXACTLY ONE ROW with FIVE equal-width cells, wide canvas. Same front-facing arch geometry, dimensions, baseline and scale in all five cells. This is ONE door opening inward on its LEFT hinges. Frame1 closed door completely filling opening. Frame2 quarter open. Frame3 half open. Frame4 three-quarters open. Frame5 fully open with narrow wooden leaf visible against LEFT interior jamb and inviting dim warm amber interior visible through the rest of arch. Draw actual perspective changes, not mere fading. The complete arch frame and one low stone threshold step must be present in every frame and stay identical; inside the arch is OPAQUE wood or a solid deep brown interior so this sprite can cover the old door painted into a background without a second door showing through. Very soft amber glow INSIDE the open doorway, no glowing rectangle outside. No surrounding wall/building, no lantern, no roof, no characters, no floor beyond the stone step, no text, no labels, no grid lines, no cast shadows outside threshold. Outside stone arch silhouette is genuine alpha transparency. Full arch including upper stones visible with 12% margins all sides. Sprite sheet intended to overlay the door area of these scenes at small scale. Do not redesign the architecture or add ornaments.

