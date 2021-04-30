<template>
  <div>
    <b-sidebar
      id="sidepanel"
      title="Node information"
      right
      shadow=true
      width="450px"
      v-model="isPanelOpen">
      <div class="px-3 py-2">
        <div id="error">
          <p>The selected node does not exist. <br><br> server_id:</p>
          <p id="errorid">"{{ errorId }}"</p>
        </div>
        <div id="info">
          <json-view :data="nodeData" :root-key="rootName"/>
        </div>
      </div>
      <div>
    <vue-tree-list
      @click="onClick"
      @change-name="onChangeName"
      @delete-node="onDel"
      @add-node="onAddNode"
      :model="data"
      default-tree-node-name="new node"
      default-leaf-node-name="new leaf"
      v-bind:default-expanded="false"
    >
      <template v-slot:leafNameDisplay="slotProps">
        <span>
          {{ slotProps.model.name }} <span class="muted">#{{ slotProps.model.id }}</span>
        </span>
      </template>
      <span class="icon" slot="addTreeNodeIcon">📂</span>
      <span class="icon" slot="addLeafNodeIcon">＋</span>
      <span class="icon" slot="editNodeIcon">📃</span>
      <span class="icon" slot="delNodeIcon">✂️</span>
      <span class="icon" slot="leafNodeIcon">🍃</span>
      <span class="icon" slot="treeNodeIcon">🌲</span>
    </vue-tree-list>
  </div>
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import SidebarPanel from 'bootstrap-vue'
import { VueTreeList, Tree, TreeNode } from 'vue-tree-list'

export default {
  name: "InfoPanel",
  components: { SidebarPanel, VueTreeList },
  data () {
    return {
      isPanelOpen: false,
      nodeData: null,
      rootName: 'node',
      errorId: ''
    }
  },
  methods: {
    onNodeClick ({nodeData, id}) {
      if (!this.$data.isPanelOpen) {
        this.$data.isPanelOpen = true
      }

      const info = document.getElementById("info")
      const error = document.getElementById("error")

      if (nodeData === '') {
        info.style.display = "none"
        error.style.display = "block"
        this.errorId = id
      } else {
        info.style.display = "block"
        error.style.display = "none"
        this.nodeData = nodeData
      }
    }
  }
}
</script>

<style scoped>

#errorid {
  word-wrap: break-word;
  color: #268bd2;
  font-weight: 600;
  margin-top: -15px;
}

#error {
  font-weight: 600;
}

</style>
