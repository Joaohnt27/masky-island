# Fontes e banner dos diálogos

As falas usam DynaPuff e os nomes usam Kablammo, ambas já presentes no projeto. A alteração cobre a introdução na praia, os pensamentos na casa e na caverna e as conversas no centro da ilha.

O NPC passa a usar o pergaminho CoastalDialogue-v1, a mesma composição da fala do player, com retrato à esquerda, nome, texto e botão para continuar. O retrato troca para o personagem quando o painel apresenta um pensamento dele. Textos têm ajuste automático limitado para acomodar falas longas.

## Arquivos modificados
- Assets/Scripts/DialogueController.cs: referências opcionais de retratos e troca conforme quem fala. Preserva os diálogos, callbacks, nomes escolhidos e início da missão.
- Assets/Scenes/Beach.unity, House.unity, Cave.unity, Island.unity: fontes, dimensões dos textos e referências de retratos; banner do NPC atualizado.

## Arquivos criados
- Tests/UIAuthoring/AuthorDialogueStyle.cs: configura as cenas na cópia isolada.
- Tests/GameplayValidation/DialogueStyleValidation.cs: verifica fontes, banner, retratos, espaço para as falas, botão e progressão do NPC.
- Este relatório, resultados e capturas de verificação.

Nenhum novo asset artístico ou fonte foi necessário. Nenhuma configuração manual pendente no Inspector.

O banner do NPC usa o GameplayMenusCanvas, acima dos sprites do cenário e abaixo dos painéis de pausa. O botão existente continua chamando NextLine; as referências ao painel foram preservadas.


## Resultado final
48 verificações aprovadas em Play Mode no Unity 6000.6.0f1. Conferidos: fontes, banner, retratos, cliques, encaixe das falas, liberação de movimento e entrega do mapa/início da missão. Capturas da fala longa do NPC e do pensamento do player revisadas visualmente, sem sprites sobre os textos. Cenas e script conferidos por hash. Backup das cenas anteriores em Builds/DialogueStyleBackup. A validação aguarda dois frames após abrir o painel para testar o raycast com o layout atualizado.

