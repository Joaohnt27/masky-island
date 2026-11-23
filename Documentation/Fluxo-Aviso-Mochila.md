# Diário → casa → aviso → centro → mochila

## Fluxo atualizado
O personagem coleta o diário na praia e anota que deve procurar ajuda na casa. Na primeira interação com a mesa, lê um aviso indicando alguém trabalhando no poço do centro. A leitura registra esse destino no diário. O NPC explica a missão, entrega o mapa e autoriza buscar sua mochila ao lado da cama, na casa próxima à praia.

A mochila pode ser examinada antes da conversa, mas o personagem não a leva sem falar com o dono. Após a autorização, coletá-la libera o inventário por clique/I e permite minerar e armazenar cristais. O objeto desaparece da casa e continua coletado entre cenas. Nova partida restaura a mochila e limpa a leitura do aviso.

O bilhete da mesa mantém o enigma existente depois da leitura do aviso e do início da missão. As áreas continuam acessíveis; o aviso dá motivação narrativa para o deslocamento.

## Arquivos
- Novo: `Assets/Scripts/BackpackPickup.cs` e `.meta`.
- Modificados: `GameManager.cs`, `JournalPickup.cs`, `QuestJournalUI.cs`, `DialogueController.cs`, `ScrollInteraction.cs`, `CrystalInventoryUI.cs`, `MiningSpot.cs`, `StoryLocation.cs`.
- Cenas: House recebe mochila e painel de fala pessoal; os cinco cenários ajustam o texto do diário para até nove tarefas.
- Novos utilitários: `Tests/UIAuthoring/AuthorBackpackFlow.cs`, `Tests/GameplayValidation/BackpackFlowValidation.cs`.

Reutiliza arte da mochila, arte de diálogo, mesa e sistemas existentes. Nenhum asset obrigatório faltante ou configuração manual no Inspector. O GameManager continua sendo a fonte do progresso; o diário usa identificadores estáveis para os riscos animados.

## Validação
24 verificações aprovadas no Unity 6 isolado, incluindo cliques com aproximação à mochila e à mesa, permissão do NPC, bloqueio do inventário e dos cristais sem mochila, coleta, persistência, reinício e nove tarefas sem transbordamento. Capturas revisadas visualmente. Compilação sem erros. A exceção conhecida do índice de busca do Editor foi registrada separadamente dos testes de gameplay. Resultados e capturas em Documentation/BackpackFlow.

