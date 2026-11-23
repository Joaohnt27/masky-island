# Picareta ilustrada

Sprite PNG com transparência, criado com image_gen integrado, combinando madeira quente, metal envelhecido, detalhes de latão e tecido verde-petróleo. A ferramenta substitui a representação feita de peças 3D durante a mineração; o sistema de golpes, áudio e progresso existente é reutilizado.

## Arquivos
- Novo: `Assets/Art/Tools/Pickaxe-v1.png` e metadados Unity.
- Modificadas: cenas de exploração, configuração do visual `MiningPickaxe`.
- Novos: `Tests/UIAuthoring/AuthorPickaxeSprite.cs`, `Tests/GameplayValidation/PickaxeSpriteValidation.cs`.
- Prompt: `Documentation/PickaxeSpritePrompt.txt`.

O sprite usa pivô no punho e é dimensionado para o personagem. Aparece durante a mineração e é ocultado ao terminar. Não há configuração manual necessária nem mudança de regras, chances ou duração da mineração.

## Validação
34 verificações aprovadas no Unity 6 isolado: aproximação e extração nas três rochas, sprite ativo durante mineração, pausa do golpe, ocultação ao terminar, inventário atualizado e acesso ao altar. Captura revisada visualmente. Compilação sem erros. A exceção conhecida do índice de busca do Editor foi registrada separadamente dos testes de gameplay. Resultados em Documentation/PickaxeSprite/ValidationResults.txt.

