using UnityEngine;
public static class InventoryItemInfo
{
 public static string Name(GameManager.ItemId id) => id switch {
 GameManager.ItemId.Triangle => "Cristal triangular", GameManager.ItemId.Circle => "Cristal circular", GameManager.ItemId.Square => "Cristal quadrado",
 GameManager.ItemId.Pickaxe => "Picareta", GameManager.ItemId.KeyBlank => "Peça de metal", GameManager.ItemId.KeyMold => "Molde antigo", _ => "Chave do farol" };
 public static string Description(GameManager.ItemId id) => id switch {
 GameManager.ItemId.Pickaxe => "Ferramenta para extrair os cristais dos veios da caverna.",
 GameManager.ItemId.KeyBlank => "Peça de latão ainda sem dentes. Use o molde na bancada da casa para ajustar a chave.",
 GameManager.ItemId.KeyMold => "O desenho de uma chave antiga. Combine com a peça de metal na bancada da casa.",
 GameManager.ItemId.LighthouseKey => "Chave fabricada na casa. Guarde-a para abrir a porta do farol.",
 _ => "Encontrado na caverna. Encaixe no símbolo correspondente do altar." };
}
