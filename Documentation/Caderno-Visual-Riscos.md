# Caderno de viagem e riscos animados

O caderno usa capa em tons de couro, bordas envelhecidas, costura central, páginas de pergaminho com linhas suaves e as fontes Kablammo (título) e DynaPuff (conteúdo). Mantém o ícone e o acesso por J/Esc.

Tarefas concluídas recebem um risco progressivo da esquerda para a direita, em sequência. Ao abrir o caderno são animadas as conclusões ainda não apresentadas. Riscos já vistos permanecem nas próximas aberturas e trocas de cenário. Fechar durante uma animação deixa a tarefa pendente de apresentação na próxima abertura. A nova partida limpa esse estado visual; o progresso continua pertencendo ao GameManager.

## Arquivos
- Modificado: `Assets/Scripts/QuestJournalUI.cs`, apresentação do texto e coroutine com tempo independente da pausa.
- Modificadas: Beach, House, Island, Cave e Lighthouse, apenas o painel do caderno.
- Novos: `Tests/UIAuthoring/AuthorJournalStyle.cs` e `Tests/GameplayValidation/JournalStyleValidation.cs`.

Sem configuração manual, fontes novas ou assets externos necessários.

## Verificação
9 verificações aprovadas no Unity 6: início e progressão do risco, conclusão, reabertura sem repetição, fontes, texto sem transbordamento, clique para fechar, troca de cena e nova partida. Capturas revisadas em 1280×720. Compilação sem erros. A exceção conhecida do índice de busca do Editor foi identificada separadamente dos testes de gameplay. Resultados e capturas em Documentation/JournalStyle.

