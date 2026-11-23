# Picareta na entrada da caverna

O NPC indica, na conversa inicial e na orientação após a primeira lembrança, que deixou a picareta logo na entrada da caverna, à esquerda, encostada nas pedras. O diário registra a busca antes da tarefa de minerar.

O jogador clica na picareta, aproxima-se e confirma a coleta pelo diálogo. É necessário ter a mochila. O objeto desaparece da entrada, a ferramenta passa a ser usada durante a mineração e a posse continua registrada ao trocar de cena. Nova partida restaura o objeto.

## Arquivos
- Novo: `Assets/Scripts/PickaxePickup.cs` e `.meta`.
- Modificados: `GameManager.cs`, `DialogueController.cs`, `MiningSpot.cs`, `QuestJournalUI.cs`, `StoryLocation.cs`.
- Cave recebe o coletável e o painel de fala pessoal; os cinco cenários ajustam o diário para dez tarefas.
- Novos utilitários: `Tests/UIAuthoring/AuthorPickaxePickup.cs`, `Tests/GameplayValidation/PickaxePickupValidation.cs`.

Reutiliza o sprite Pickaxe-v1 e a arte de diálogo existentes. Nenhuma configuração manual no Inspector ou asset adicional necessário.

## Validação
13 verificações aprovadas no Unity 6: indicação do NPC, requisito da mochila, bloqueio antes da coleta, aproximação por clique, diálogo, coleta, mineração, animação da ferramenta, recompensa, dez tarefas no diário, retorno à caverna e nova partida. Capturas revisadas visualmente. Compilação sem erros. A exceção conhecida do índice de busca do Editor foi registrada separadamente dos testes de gameplay. Resultados e capturas em Documentation/PickaxePickup.

