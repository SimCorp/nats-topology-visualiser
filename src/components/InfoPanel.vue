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
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import SidebarPanel from 'bootstrap-vue'
export default {
  name: "InfoPanel",
  components: { SidebarPanel },
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
