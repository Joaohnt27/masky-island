# Diário ilustrado e coleta na praia

## Arte e interface
- `Assets/Art/UI/Journal/OpenJournal-v1.png`: arte original gerada com image_gen integrado. O livro aberto substitui os retângulos da interface; textos e riscos continuam em TextMeshPro sobre as páginas.
- Prompt completo: `Documentation/JournalArtPrompt.txt`.

## Fluxo
1. O personagem nota um livro na areia durante a introdução da praia.
2. O jogador clica no livro para se aproximar, examiná-lo e descobrir que está vazio.
3. Ao terminar essas falas, leva o diário. O objeto desaparece da praia e o ícone J passa a ficar disponível. As páginas permanecem vazias.
4. Depois da conversa com o estranho, o personagem comenta que vai registrar as pistas e riscar o que terminar. O diário passa a apresentar tarefas e lembranças.
5. Se a coleta ocorrer após a conversa com o estranho, o comentário de aprendizado acontece ao pegar o diário.

## Arquivos
- Criados: `JournalPickup.cs` e meta; arte e metadados; `AuthorDiaryPickup.cs` e `DiaryPickupValidation.cs` em Tests.
- Modificados: `GameManager.cs` (posse e aprendizado, reinício), `DialogueController.cs` (falas pessoais reutilizando painel e bloqueio de movimento), `QuestJournalUI.cs` (acesso condicionado à coleta e páginas vazias antes do aprendizado).
- Cenas: praia recebe o objeto coletável; os cinco cenários recebem o painel ilustrado.

Nenhuma configuração manual no Inspector é necessária. Os estados persistem pelo GameManager durante a partida e são limpos ao reiniciar. Nenhum asset obrigatório faltante.

## Validação
12 verificações aprovadas no Unity 6 isolado: clique no cenário com aproximação automática, diálogo e coleta, ícone condicionado à posse, páginas vazias, aprendizado após NPC, preservação de mapa/missão, encaixe dos textos na arte, botão de fechar, retorno à praia, reinício e coleta após a conversa. Compilação sem erros. Capturas revisadas visualmente. O índice de busca do Editor registrou sua exceção conhecida, separada dos testes de gameplay. Resultados em Documentation/DiaryPickup/ValidationResults.txt.

