# Etapa: inventário de itens, esconderijo e chave

## Fluxo implementado
1. Os cristais são guardados como itens na mochila.
2. Cada encaixe correto consome seu cristal do inventário, mantendo o cristal visível no altar e o histórico de coleta.
3. O terceiro encaixe abre o esconderijo de pedra junto ao altar. Abrir não entrega materiais automaticamente.
4. Um clique no esconderijo aproxima o personagem. Ao concluir a fala de coleta, a peça de metal e o molde antigo entram na mochila.
5. Na casa, clicar na mesa/bancada após essa etapa apresenta a fabricação. Ao concluir, os materiais são consumidos e a chave do farol é adicionada. O antigo terceiro puzzle deixa de abrir.

## Inventário
O GameManager continua sendo a fonte persistente do progresso. Um dicionário de tipos de item e quantidades substitui a coleção restrita aos cristais; os encaixes guardam o histórico dos minerais usados. A picareta também aparece na mochila. A interface existente foi adaptada, mantendo I, Esc e os botões. Seis espaços visuais acomodam os itens atuais; selecionar um item mostra sua descrição. Não há um segundo InventoryManager.

## Arquivos novos
- Assets/Scripts/InventoryItemInfo.cs: nomes e descrições.
- Assets/Scripts/CaveKeyCache.cs: abertura e coleta do esconderijo usando a interação existente.
- Assets/Art/UI/KeyCrafting-v1.png e meta: peça de metal, molde, chave e duas poses da tampa. Arte gerada pela ferramenta integrada imagegen; prompt em art-prompt.txt.
- Tests/UIAuthoring/AuthorKeyInventory.cs e Tests/GameplayValidation/KeyInventoryValidation.cs: autoria e validação na cópia isolada.

## Arquivos modificados
GameManager, CrystalInventoryUI, ScrollInteraction, StoryLocation, QuestJournalUI, MiningUI e DialogueController: itens, fabricação, objetivos e orientações. As cinco cenas recebem a nova mochila; Cave recebe o esconderijo; House desativa o puzzle antigo. Os sistemas de mineração, encaixes e diálogos são reutilizados.

## Configuração e próximos passos
Nenhuma configuração manual necessária no Inspector após importar os assets e cenas. Persistência durante trocas de cena, conforme o sistema atual; não foi adicionado salvamento em disco.

O exterior do farol e o cadeado de senha pertencem à próxima etapa. Nesta entrega o caminho atual para o interior e o QTE permanecem disponíveis após fabricar a chave; a chave ainda não é consumida em uma porta nova.

## Resultado da validação
37 verificações aprovadas em Play Mode. Cobertura: inventário de cristais e picareta, encaixe errado, consumo no encaixe certo, abertura após três cristais, coleta manual por clique com aproximação, proteção contra duplicação, persistência entre cenas, fabricação na casa, consumo dos materiais, chave única, retorno à caverna com esconderijo vazio e reinício. Capturas da mochila, esconderijo e diário revisadas. Cenas, scripts e imagem conferidos por hash. Backup das cenas em Builds/KeyInventoryBackup. Não houve erro de gameplay; a exceção conhecida do índice de busca do Editor é registrada separadamente pelo validador.

