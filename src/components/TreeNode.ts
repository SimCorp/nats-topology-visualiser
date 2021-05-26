export default class TreeNode {
  name!: string
  id!: number
  server_id!: string
  pid!: number
  dragDisabled!: boolean
  addTreeNodeDisabled!: boolean
  addLeafNodeDisabled!: boolean
  editNodeDisabled!: boolean
  delNodeDisabled!: boolean
  isLeaf!: boolean
  children!: TreeNode[]
}