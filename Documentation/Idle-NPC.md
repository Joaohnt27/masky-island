# Idle do NPC

O Estranho mantém a postura frontal junto ao poço, com respiração discreta em um ciclo de três segundos. Os quatro quadros são reproduzidos na ordem 0, 1, 2, 3, 2, 1 e retornam ao início. Os pés são alinhados pelos pivôs; a animação troca apenas o sprite, sem deslocar o NPC, o colisor ou o ponto de aproximação.

## Arquivos criados

- `Assets/Art/Characters/NPC/NpcIdle-v1.png` e `.meta`: folha de quatro poses com transparência.
- `Assets/Animations/NPC/NpcIdle.anim` e `.meta`: clip em loop.
- `Assets/Animations/NPC/NPC.controller` e `.meta`: controlador com estado Idle.
- `Tests/UIAuthoring/AuthorNpcIdle.cs`: importação, recorte, criação dos assets e configuração da cena na cópia de autoria.
- `Tests/GameplayValidation/NpcIdleValidation.cs`: validação do idle, pausa, clique, diálogo e retorno ao centro.

## Arquivos modificados

- `Assets/Scenes/Island.unity`: sprite inicial e Animator no objeto visual NPC. O sistema de interação, diálogos e as animações do jogador são preservados.

## Configuração no Unity

Nenhuma configuração manual necessária após a integração. O Animator usa tempo normal do jogo, sem root motion. A pausa congela o ciclo, que continua ao retomar. A respiração também permanece durante as conversas.

Nenhum asset adicional é necessário. O sprite de referência original não foi sobrescrito.

## Validação concluída

13 verificações aprovadas no Unity 6000.6.0f1: controlador e início automático, quatro poses, posição e escala estáveis, colisor fixo, pausa e retomada, clique e diálogo, respiração durante conversa, início da missão, entrega do mapa, retorno ao centro e preservação da animação do jogador. Compilação sem erros. A exceção preexistente do índice de busca do Editor é filtrada apenas quando originada em UnityEditor.Search.SearchDatabase.

Resultados: `Documentation/NpcIdle/ValidationResults.txt`. Captura revisada visualmente: `Documentation/NpcIdle/npc-idle-center.png`. Os sete arquivos integrados foram comparados por hash com a versão testada. A cena anterior foi preservada em `Builds/NpcIdleBackup/Island.unity`.

## Arte

Gerada pela ferramenta de imagens integrada, com fundo transparente, usando `Assets/Art/Backgrounds/npc_sprite.png` como referência. A folha foi copiada para o projeto.

Prompt utilizado:

Create a production-ready idle breathing animation sprite sheet for the FRONT-FACING red NPC in the attached reference. Preserve exact character identity and illustrated game style: tiny traveler with oversized round ivory mask, two dark oval eye holes, small mouth dot, ochre-gold rim, deep red hood and knee-length red coat with central seam, brown mittens and brown boots. Fine dark ink outline, subtle painted texture. ALL FOUR frames must face straight towards the camera; mask and eyes unchanged, BOTH BOOTS planted side by side, arms relaxed. Exact regular grid ONE ROW of FOUR equal square cells on wide 4:1 canvas. Character centered in each cell with identical foot baseline, same head size and same proportions. Frame 1 relaxed exhaled, frame 2 beginning inhale shoulders rise minimally, frame 3 gentle inhale torso slightly expands, frame 4 full inhale shoulders a little higher with tiny hood movement. These are subtle incremental changes, no walking, no waving, no blinking the mask holes, no turning. Designed to play 1 2 3 4 3 2 in a quiet loop. Full character visible occupying about 75 percent of each cell height with generous margins. Genuine TRANSPARENT background, no shadows, no floor, no text, no labels, no grid lines, no scene, no extra items. Match the red NPC reference, NOT the teal player.

