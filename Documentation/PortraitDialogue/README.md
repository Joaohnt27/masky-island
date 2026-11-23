# Diálogos com retrato em destaque

Composição inspirada na referência enviada: retrato grande à direita, parcialmente atrás do topo da caixa de fala, e pergaminho compacto na parte inferior. Mantidas DynaPuff nas falas e Kablammo nos nomes, com cor quente para identificar o interlocutor. Player e Estranho usam retratos próprios e a troca de interlocutor já existente.

## Arquivos criados
- Assets/Art/UI/DialoguePortraits-v1.png e meta: dois retratos transparentes gerados com a ferramenta integrada imagegen, usando os personagens atuais como referência. Recortes do atlas feitos pelo importador de sprites do Unity.
- Assets/Art/UI/DialoguePaper-v2.png e meta: cópia da arte costeira existente, com importação em nove fatias para adaptar o centro sem deformar as bordas.
- Tests/UIAuthoring/AuthorPortraitDialogue.cs: importação e aplicação às quatro cenas de diálogo.
- Tests/GameplayValidation/PortraitDialogueValidation.cs: validação de fontes, imagens, cliques, falas e progressão.

## Arquivos modificados
Beach.unity, House.unity, Cave.unity e Island.unity: posição e tamanho dos elementos, retratos e arte da caixa. Não há mudança nos textos ou regras dos diálogos.

Nenhuma configuração manual necessária no Inspector. Nenhum asset pendente. Prompt de geração em art-prompt.txt; geração pela ferramenta integrada, sem CLI.

## Validação final
48 verificações aprovadas em Play Mode no Unity 6000.6.0f1. Capturas do player, do NPC, dos pensamentos e da fala longa revisadas visualmente. Cenas e imagens conferidas por hash. Backup das cenas anteriores em Builds/PortraitDialogueBackup.

