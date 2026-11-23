# Etapa 03 — primeira lembrança

## Entrega

O pergaminho existente sobre a pedra no centro de Island passa a apresentar o primeiro puzzle. O jogador conversa com o NPC, aproxima-se do pergaminho, aperta E e lê:

> O sol se despede. A lua desperta. A estrela mostra o caminho.

A sequência SOL → LUA → ESTRELA revela **“Você cuidava da luz.”**. Os botões aparecem em outra ordem visual, exigindo seguir a pista. Uma escolha errada reinicia a tentativa, sem punição. Fechar e reabrir também reinicia uma tentativa incompleta.

A primeira lembrança não termina a missão. Os outros dois puzzles e o farol continuam para as próximas etapas. Não foram reintroduzidos moedas, timer ou derrota por prazo.

## Arquivos criados

- Assets/Scripts/SymbolSequencePuzzle.cs e .meta: sequência configurável, feedback, integração com a memória global e bloqueio de novas recompensas.
- Tests/UIAuthoring/AuthorFirstPuzzle.cs: autoria do painel com a API do Editor Unity, executada em cópia isolada.
- Tests/GameplayValidation/FirstPuzzleValidation.cs: testes específicos da primeira parte jogável.
- Tests/GameplayValidation/RunFirstPuzzle.ps1: execução reproduzível em cópia isolada.
- Documentation/FirstPuzzleValidationResults.txt: resultado dos 34 testes da execução final.
- Documentation/FirstPuzzlePreview.png e FirstMemoryPreview.png: painel renderizado pela Unity antes/depois da solução.
- Este relatório.

## Arquivos modificados

- Assets/Scripts/GameManager.cs: guarda IDs de lembranças em um conjunto, rejeita recompensas repetidas e limpa o progresso ao reiniciar/voltar ao menu. Usa o evento de estado existente, sem guardar objetos de cena.
- Assets/Scripts/DialogueController.cs: introdução com a localização do pergaminho, dica em conversas posteriores e reconhecimento da primeira lembrança.
- Assets/Scenes/Island.unity: componente do puzzle, três botões, texto de feedback, pista, título e ajuste do Canvas exclusivo do pergaminho para escala responsiva. O botão FECHAR continua chamando ScrollInteraction.CloseScroll.
- Tests/GameplayValidation/GameplayValidationRunner.cs: evita executar a suíte geral junto com a suíte específica na mesma sessão de testes.
- Tests/GameplayValidation/Run.ps1: reduz o caminho da cópia de testes para evitar erros de caminho longo do Windows ao importar pacotes. As cópias ficam em Builds, pois fechar o Editor principal pode limpar Temp.

## Comportamento

- Antes do diálogo: pode ler o pergaminho, mas os símbolos ficam bloqueados com orientação para falar com o NPC.
- Durante o puzzle: movimento bloqueado pelo ScrollInteraction existente. Pausa bloqueia os botões e conserva a sequência parcial.
- Ao acertar: mostra a frase e o total de lembranças; bloqueia os símbolos para impedir repetição.
- Ao trocar entre Cave e Island: preserva a lembrança e restaura o painel resolvido.
- Ao reiniciar ou retornar ao menu: limpa as lembranças e restaura o início da aventura.
- Persistência é apenas durante a partida. Não foi implementado salvamento em disco.

## Configuração no Unity

As referências são salvas na cena, sem configuração manual para jogar. Para inspecionar:

- Island → UI → ScrollPanel → SymbolSequencePuzzle: Sun Button, Moon Button, Star Button e Feedback Text atribuídos.
- Solution: Sun, Moon, Star. Memory: LighthouseKeeper. Memory Text: “Você cuidava da luz.”.
- Scroll_Interactable continua com Player, Interaction Prompt, Scroll Panel e Player Movement existentes.
- Cada botão usa um callback persistente: SelectSun, SelectMoon ou SelectStar.
- O Canvas do pergaminho usa Scale With Screen Size, 1280 × 720, Expand.

## Assets

Reutilizados o pergaminho e a fonte TMP existentes. Os símbolos são representados por nomes nos botões; não dependem de emojis ou sprites novos. Cristais, animação de mineração e apresentação do farol/final ficam para etapas posteriores.

## Validação

- Compilação C# e compilação pela Unity aprovadas, sem erros de código.
- **34 verificações aprovadas em Play Mode**, incluindo pré-requisito do NPC, última fala, callbacks dos botões, erros no primeiro/segundo símbolo, pausa, reabertura, acerto, recompensa única, reconhecimento pelo NPC, persistência Cave/Island, ausência de GameManager duplicado, reset e saída para o menu.
- Textos do painel verificados quanto ao tamanho disponível. Prévia renderizada em 1280 × 720 inspecionada antes/depois da solução. Corrigidos a gravação das três linhas da pista e a fonte/contraste do botão FECHAR.
- Cena final e script do puzzle idênticos aos da cópia validada, confirmados por hash. Referências locais e scripts próprios de Cave/Island conferidos sem pendências.
- A Unity apresentou uma exceção conhecida do índice de busca do Editor na inicialização batch. Ela foi registrada como EDITOR NOTE, separadamente; exceções de gameplay continuam causando falha. O relatório completo está em FirstPuzzleValidationResults.txt; o log bruto fica em Builds/PV/first-puzzle-playmode.log.
- Os testes automatizados acionam os métodos de interação e os callbacks reais dos botões. A navegação manual pelo cenário com mouse/tecla E e outras proporções de tela ainda devem ser experimentadas no Editor.

Para repetir a suíte, executar na raiz do projeto:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/GameplayValidation/RunFirstPuzzle.ps1
```

A suíte cria uma cópia independente em Builds/PV-<id> e salva nela o relatório e as prévias. A cópia utilizada nesta entrega está em Builds/PV.

## Como experimentar

1. Inicie pelo menu e saia de Cave para Island.
2. Aproxime-se do NPC e aperte E. Termine as seis falas.
3. Vá até o pergaminho sobre a pedra central e aperte E.
4. Tente uma ordem errada; depois clique SOL, LUA e ESTRELA.
5. Leia a lembrança, feche o painel e converse novamente com o NPC.
6. Volte a Cave e retorne a Island: a lembrança deve continuar registrada.

Próxima entrega: puzzle dos cristais com mineração determinística e segunda lembrança.
