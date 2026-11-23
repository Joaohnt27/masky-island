# Animações do personagem

## Comportamento

- W: caminha de costas, para o fundo do cenário.
- S: caminha de frente, em direção ao jogador.
- A e D: caminham em perfil para esquerda e direita.
- Ao parar, mantém a última direção e entra no idle correspondente.
- A aproximação automática de objetos clicados usa as mesmas animações.
- A animação observa o deslocamento real, não apenas a tecla pressionada. Limites do mapa e bloqueios de interação não deixam o personagem andando parado.
- A pausa congela a animação. Chegadas e teletransportes não são tratados como passos.

## Assets e integração

`Assets/Art/Characters/Player/PlayerWalk-v1.png`: folha transparente com seis quadros por direção.

`Assets/Art/Characters/Player/PlayerIdle-v1.png`: duas poses de repouso por direção.

`Assets/Animations/Player`: oito AnimationClips em loop e um Animator Controller compartilhado pelos cinco cenários. Caminhada em 8 quadros por segundo; idle em 2 quadros por segundo. Os sprites são alinhados pelos pés na importação.

`Assets/Scripts/PlayerSpriteAnimation.cs`: seleciona os estados usando o deslocamento no plano XY. Preserva a orientação nas diagonais para evitar trocas rápidas entre perfis. Não altera posições, colisores ou a lógica de interação.

As cenas Beach, House, Island, Cave e Lighthouse recebem o controlador no Player. A folha de referência antiga e os retratos de diálogo são preservados. Nenhum pacote novo é necessário.

## Autoria e verificação

`Tests/UIAuthoring/AuthorPlayerAnimation.cs` importa e recorta as folhas, cria os clips/controlador e configura as cenas. Executado na cópia de validação `Builds/PV`, seguindo o fluxo de autoria existente.

`Tests/GameplayValidation/PlayerAnimationValidation.cs` verifica movimento com New Input System, troca de quadros, idle direcional, bloqueios, bordas, pausa, interação por clique e referências nos cinco cenários. Os resultados ficam em `Documentation/PlayerAnimation` após a execução.

Nenhuma atribuição manual no Inspector é necessária após a cópia dos assets e cenas validados. A animação da picareta continua usando o sistema de mineração existente.

Resultado: 36 verificações aprovadas no Unity 6000.6.0f1, sem erros de compilação. Capturas das quatro direções e do idle na caverna revisadas visualmente. A exceção preexistente do índice de busca do Editor foi desconsiderada pelo teste apenas quando originada em `UnityEditor.Search.SearchDatabase`, sem ocultar falhas do gameplay.

Arquivos modificados: somente as cinco cenas de exploração, para adicionar Animator e PlayerSpriteAnimation e trocar o sprite inicial. Arquivos criados: as duas folhas PNG e seus metadados, os oito clips e controlador em `Assets/Animations/Player`, o novo script e metadado, os dois utilitários em `Tests` e este relatório com evidências em `Documentation/PlayerAnimation`. Não há assets faltantes. Uma cópia das cenas anteriores foi guardada em `Builds/PlayerAnimationBackup`.

## Arte e prompts

Ferramenta utilizada: geração de imagens integrada, com `transparent_background: true`. Referência: `Assets/Art/Backgrounds/player_sprites.png`. As imagens geradas foram copiadas para o projeto, sem substituir a referência original.

Prompt da caminhada: folha de sprites para Unity, personagem viajante ilustrado com máscara redonda branca, olhos ovais escuros, capuz e casaco verde-petróleo, contorno ocre, luvas e botas marrons; preservar a identidade da referência. Grade regular de seis colunas por quatro linhas. Cada linha mostra um ciclo de caminhada na mesma direção: frente, costas sem rosto, esquerda, direita. Alternar pernas e braços, manter proporções, escala e alinhamento dos pés; fundo transparente, sem texto, divisórias, objetos, chão ou sombras.

Prompt do idle: mesmo personagem e acabamento desenhado; grade de quatro colunas por duas linhas, com as direções frente, costas, esquerda e direita nas colunas. Primeira linha em repouso; segunda linha com respiração discreta. Pés apoiados, braços relaxados, sem caminhar nem mudar de direção. Escala uniforme, margens transparentes, sem textos, chão ou sombras.
